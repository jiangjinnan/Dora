using Dora.GraphQL.Resolvers;
using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.GraphQL.GraphTypes
{
    public class GraphType : IEquatable<GraphType>, IGraphType
    {
        private readonly Func<object,object> _valueResolver;
        public Type Type { get; }
        public Type[] OtherTypes { get; }
        public string Name { get; }
        public bool IsRequired { get; }
        public bool IsEnumerable { get; }
        public bool IsEnum { get; }
        public IDictionary<NamedType, GraphField> Fields { get; }
        private GraphType(OperationType operationType)
        {
            Type = typeof(void);
            OtherTypes = Type.EmptyTypes;
            Name = operationType.ToString();
            IsEnumerable = false;
            IsRequired = false;
            Fields = new Dictionary<NamedType, GraphField>();
        }

        internal GraphType(
            IGraphTypeProvider graphTypeProvider,
            IAttributeAccessor attributeAccessor,
            Type type,
            bool? isRequired,
            bool? isEnumerable,
            Type[] otherTypes)
        {
            Guard.ArgumentNotNull(graphTypeProvider, nameof(graphTypeProvider));
            Guard.ArgumentNotNull(type, nameof(type));
            if (otherTypes == null || otherTypes.Length == 0)
            {
                OtherTypes = Type.EmptyTypes;
            }
            else
            {
                foreach (var otherType in otherTypes)
                {
                    if (otherType.IsEnumerableType(out _))
                    {
                        throw new ArgumentException($"Union GraphType cannnot be created based on enumerable type 'otherType'", nameof(otherTypes));
                    }
                }
                OtherTypes = otherTypes;
            }

            var isEnumerableType = type.IsEnumerableType(out var clrType);
            clrType = clrType ?? type;
            _valueResolver = GraphValueResolver.GetResolver(clrType);
            if (isEnumerableType && isEnumerable == false)
            {
                throw new GraphException($"Cannot create non-enumerable GraphType based on the type '{type}'");
            }
            
            Type = clrType;
            IsRequired = isRequired??false;
            IsEnumerable = isEnumerable?? isEnumerableType;
            IsEnum = clrType.IsEnum;
            var name = GraphValueResolver.GetGraphTypeName(clrType, otherTypes);
            var requiredFlag = IsRequired ? "!" : "";
            Name = IsEnumerable
                ? $"[{name}]{requiredFlag}"
                : $"{name}{requiredFlag}";

            Fields = new Dictionary<NamedType, GraphField>();
            if (!GraphValueResolver.IsScalar(clrType))
            {
                foreach (var item in GetProperties(attributeAccessor))
                {
                    var property = item.Property;
                    var memberAttribute = attributeAccessor.GetAttribute<GraphMemberAttribute>(property, false);
                    var resolver = memberAttribute?.Resolver == null
                        ? (IGraphResolver)new PropertyResolver(property)
                        : new MethodResolver(Type.GetMethod(memberAttribute.Resolver));

                    var isPropertyEnumerable = property.PropertyType.IsEnumerableType(out var propertyType);
                    propertyType = propertyType ?? property.PropertyType;
                    var isPropertyRequired = memberAttribute?.IsRequired == true;

                    var unionTypeAttribute = attributeAccessor.GetAttribute<UnionTypeAttribute>(property, false);
                    var propertyGraphType = unionTypeAttribute == null
                        ? new GraphType(graphTypeProvider, attributeAccessor, propertyType, isPropertyRequired, isPropertyEnumerable,Type.EmptyTypes)
                        : new GraphType(graphTypeProvider,attributeAccessor, unionTypeAttribute.Type, isPropertyRequired, isPropertyEnumerable, unionTypeAttribute.OtherTypes);

                    if (unionTypeAttribute != null)
                    {
                        Array.ForEach(unionTypeAttribute.Types, it => new GraphType(graphTypeProvider, attributeAccessor, it, false, false, Type.EmptyTypes));
                    }

                    //Arguments
                    var fieldName = item.MemberName;
                    var field = new GraphField(fieldName, propertyGraphType, property.DeclaringType, resolver);
                    var argumentAttributes = attributeAccessor.GetAttributes<ArgumentAttribute>(property, false);
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

                        var isEnumerableArgument = attribute.Type.IsEnumerableType(out var argumentType);
                        argumentType = argumentType ?? attribute.Type;
                        var argumentGraphType = new GraphType(graphTypeProvider,attributeAccessor, argumentType, attribute.IsRequired, attribute.GetIsEnumerable() ?? isEnumerableArgument,Type.EmptyTypes);
                        var argument = new NamedGraphType(attribute.Name, argumentGraphType);
                        field.AddArgument(argument);
                    }
                    Fields.Add(new NamedType(fieldName, property.DeclaringType), field);
                }
            }

            graphTypeProvider.AddGraphType(Name, this);

            var knownTypes = attributeAccessor.GetAttributes<KnownTypesAttribute>(type, false)
                .SelectMany(it => it.Types)
                .ToArray();
            foreach (var knownType in knownTypes)
            {
                var _= new GraphType(graphTypeProvider, attributeAccessor, knownType, null, null, Type.EmptyTypes);
            }
        }
        public bool Equals(GraphType other)
        {
            return other != null && other.Name == Name;
        }
        public override int GetHashCode() => Name.GetHashCode();
        internal static GraphType CreateGraphType(OperationType operationType) => new GraphType(operationType);       
        public object Resolve(object rawValue) => _valueResolver(rawValue);
        private List<(string MemberName, PropertyInfo Property)> GetProperties(IAttributeAccessor attributeAccessor)
        {
            var list = new List<(string MemberName, PropertyInfo Property)>();
            void Collect(Type type)
            {
                foreach (var property in type.GetProperties())
                {
                    var memberAttribute = attributeAccessor.GetAttribute<GraphMemberAttribute>(property, false);
                    var name = memberAttribute?.Name ?? property.Name;
                    if (memberAttribute?.Ignored == true)
                    {
                        continue;
                    }
                    list.Add((memberAttribute?.Name ?? property.Name, property));
                }
            }
            Collect(Type);
            Array.ForEach(OtherTypes, it => Collect(it));
            return list;
        }
    }
}
