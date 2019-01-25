using Dora.GraphQL.Resolvers;
using Dora.GraphQL.Schemas;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// The default implementation class of <see cref="IGraphTypeProvider"/>.
    /// </summary>
    public class GraphTypeProvider : IGraphTypeProvider
    {
        #region Fields
        private readonly IServiceProvider _serviceProvider;
        private readonly IAttributeAccessor _attributeAccessor;
        private Dictionary<string, IGraphType> _graphTypes;
        private readonly FieldNameConverter _fieldNameConverter;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeProvider" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="attributeAccessor">The attribute accessor.</param>
        /// <param name="optionsAccessor">The options accessor.</param>
        public GraphTypeProvider(IServiceProvider serviceProvider, IAttributeAccessor attributeAccessor, IOptions<GraphOptions> optionsAccessor)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _attributeAccessor = attributeAccessor ?? throw new ArgumentNullException(nameof(attributeAccessor));
            _graphTypes = new Dictionary<string, IGraphType>();
            _fieldNameConverter = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.FieldNameConverter;
            foreach (var scalarType in GraphValueResolver.ScalarTypes)
            {                
                GetGraphType(scalarType, null, false);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tries to get the created <see cref="IGraphType" /> based on specified GraphQL type name.
        /// </summary>
        /// <param name="name">The GraphQL type name.</param>
        /// <param name="graphType">The <see cref="IGraphType" />.</param>
        /// <returns>
        /// A <see cref="T:System.Boolean" /> value indicating whether to successfully get the areadly created <see cref="IGraphType" />.
        /// </returns>
        public bool TryGetGraphType(string name, out IGraphType graphType)
        {
            Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            return _graphTypes.TryGetValue(name, out graphType);
        }

        /// <summary>
        /// Create a new <see cref="IGraphType" /> or get an existing <see cref="IGraphType" /> based on the given CLR type.
        /// </summary>
        /// <param name="type">The <see cref="IGraphType" /> specific CLR type.</param>
        /// <param name="isRequired">Indicate whether to create a required based <see cref="IGraphType" />.</param>
        /// <param name="isEnumerable">Indicate whether to create an array based <see cref="IGraphType" />.</param>
        /// <param name="otherTypes">The other CLR types for union GraphQL type.</param>
        /// <returns>
        /// The <see cref="IGraphType" /> to be created to provided.
        /// </returns>
        public IGraphType GetGraphType(Type type, bool? isRequired, bool? isEnumerable, params Type[] otherTypes)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            type = GetValidType(type);
            EnsureValidUnionTypes(otherTypes);

            if (GraphValueResolver.IsRequired(type) == true && isRequired == false)
            {
                throw new GraphException($"Cannot create optional GraphType based on the type '{type}'");
            }
            if (GraphValueResolver.IsRequired(type) == false && isRequired == true)
            {
                throw new GraphException($"Cannot create required GraphType based on the type '{type}'");
            }

            var required = isRequired ?? GraphValueResolver.IsRequired(type) ?? false;

            var isEnumerableType = type.IsEnumerable(out var elementType);
            if (isEnumerableType && isEnumerable == false)
            {
                throw new GraphException($"Cannot create non-enumerable GraphType based on the type '{type}'");
            }
            var clrType = isEnumerableType ? elementType : type;
            var enumerable = isEnumerable ?? isEnumerableType;
            var isEnum = clrType.IsEnum;
            var name = GraphValueResolver.GetGraphTypeName(clrType, otherTypes);
            var requiredFlag = required == true ? "!" : "";
            var valueResolver = GraphValueResolver.GetResolver(clrType, _serviceProvider);
            name = enumerable
                ? $"[{name.TrimEnd('!')}]{requiredFlag}"
                : $"{name.TrimEnd('!')}{requiredFlag}";

            if (_graphTypes.TryGetValue(name, out var value))
            {
                return value;
            }
            var graphType = new GraphType(valueResolver, type, otherTypes, name, required, enumerable, isEnum);
            _graphTypes[name] = graphType;
            if (!GraphValueResolver.IsScalar(type))
            {
                foreach (var (fieldName, property) in GetProperties(type, otherTypes))
                {
                    var memberAttribute = _attributeAccessor.GetAttribute<GraphFieldAttribute>(property, false);
                    var resolver = GetPropertyResolver(type, property, memberAttribute);
                    var propertyGraphType = GetPropertyGraphType(type, property, memberAttribute);
                    var normalizedFieldName = _fieldNameConverter.Normalize(fieldName);
                    var field = new GraphField(normalizedFieldName, propertyGraphType, property.DeclaringType, resolver);
                    foreach (var argument in GetPropertyArguments(property))
                    {
                        field.AddArgument(argument);
                    }
                    graphType.AddField(property.DeclaringType, field);
                }
            }

            var knownTypeAttribute = _attributeAccessor.GetAttribute<KnownTypesAttribute>(type,false);
            if (knownTypeAttribute != null)
            {
                Array.ForEach(knownTypeAttribute.Types, it => GetGraphType(it, false, false));
            }
            return graphType;
        }
        #endregion

        #region Private methods
        private IGraphType GetPropertyGraphType(Type type, PropertyInfo property, GraphFieldAttribute memberAttribute)
        {
            var isPropertyEnumerable = property.PropertyType.IsEnumerable(out var propertyType);
            propertyType = propertyType ?? property.PropertyType;
            var isPropertyRequired = memberAttribute?.IsRequired;

            var unionTypeAttribute = _attributeAccessor.GetAttribute<UnionTypeAttribute>(property, false);
            var propertyGraphType = unionTypeAttribute == null
                ? GetGraphType(propertyType, isPropertyRequired, isPropertyEnumerable)
                : GetGraphType(unionTypeAttribute.Type, isPropertyRequired, isPropertyEnumerable, unionTypeAttribute.OtherTypes);
            if (unionTypeAttribute != null)
            {
                Array.ForEach(unionTypeAttribute.Types, it => GetGraphType(it, false, false));
            }
            return propertyGraphType;
        }

        private IGraphResolver GetPropertyResolver(Type type, PropertyInfo property, GraphFieldAttribute memberAttribute)
        {
            if (!string.IsNullOrEmpty(memberAttribute?.Resolver))
            {
                var resolverMethod = type.GetMethod(memberAttribute.Resolver);
                if (null == resolverMethod)
                {
                    throw new GraphException($"The specified custom resolver method '{memberAttribute.Resolver}' is not defined in the type '{type}'");
                }
                return new MethodResolver(resolverMethod);
            }
            return new PropertyResolver(property);
        }

        private IEnumerable<NamedGraphType> GetPropertyArguments(PropertyInfo property)
        {
            var argumentAttributes = _attributeAccessor.GetAttributes<ArgumentAttribute>(property, false);
            foreach (var attribute in argumentAttributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    throw new GraphException("Does not specifiy the Name property of the ArgumentAttribute annotated with property member.");
                }

                if (attribute.Type == null)
                {
                    throw new GraphException("Does not specifiy the Type property of the ArgumentAttribute annotated with property member.");
                }

                var isEnumerableArgument = attribute.Type.IsEnumerable(out var argumentType);
                argumentType = argumentType ?? attribute.Type;
                var argumentGraphType = GetGraphType(argumentType, attribute.IsRequired, attribute.GetIsEnumerable() ?? isEnumerableArgument);
                yield return new NamedGraphType(attribute.Name, argumentGraphType);
            }
        }

        private static void EnsureValidUnionTypes(Type[] otherTypes)
        {
            foreach (var otherType in otherTypes)
            {
                if (otherType.IsEnumerable(out _))
                {
                    throw new ArgumentException($"Union GraphType cannnot be created based on enumerable type 'otherType'", nameof(otherTypes));
                }
            }
        }

        private List<(string MemberName, PropertyInfo Property)> GetProperties(Type type, Type[] otherTypes)
        {
            var list = new List<(string MemberName, PropertyInfo Property)>();
            void Collect(Type t)
            {
                foreach (var property in t.GetProperties())
                {
                    var memberAttribute = _attributeAccessor.GetAttribute<GraphFieldAttribute>(property, false);
                    var name = memberAttribute?.Name ?? property.Name;
                    if (memberAttribute?.Ignored == true)
                    {
                        continue;
                    }
                    list.Add((memberAttribute?.Name ?? property.Name, property));
                }
            }
            Collect(type);
            Array.ForEach(otherTypes, it => Collect(it));
            return list;
        }

        private static Type GetValidType(Type type)
        {
            if (type == typeof(Task) && type == typeof(void) && type == typeof(ValueTask))
            {
                throw new GraphException("GraphType cannot be created based on type Task, ValueTask or void");
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GenericTypeArguments[0];
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return type.GenericTypeArguments[0];
            }

            return type;
        }
        #endregion
    }
}
