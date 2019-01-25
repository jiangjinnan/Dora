using Dora.GraphQL;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using GraphQL.Types;
using GraphQL.Utilities;
using System;
using System.Linq;
using IGraphType = Dora.GraphQL.GraphTypes.IGraphType;
using IGraphTypeOfGraphQLNet = GraphQL.Types.IGraphType;

namespace Dora.GraphQL.Server
{
    public class GraphSchemaConverter : IGraphSchemaConverter
    {
        private readonly IGraphTypeProvider _graphTypeProvider;
        public GraphSchemaConverter(IGraphTypeProvider graphTypeProvider)
        {
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
        }
        public GraphQLNetSchema Convert(IGraphSchema graphSchema)
        {
            var schema = new GraphQLNetSchema();
            if (schema.Query.Fields.Any())
            {
                schema.Query = (IObjectGraphType)Convert(graphSchema.Query);
            }
            if (schema.Mutation.Fields.Any())
            {
                schema.Mutation = (IObjectGraphType)Convert(graphSchema.Mutation);
            }
            if (schema.Subscription.Fields.Any())
            {
                schema.Subscription = (IObjectGraphType)Convert(graphSchema.Subscription);
            }
            return schema;
        }
        private IGraphTypeOfGraphQLNet Convert(IGraphType graphType)
        {
            //Enumerable
            if (graphType.IsEnumerable)
            {
                var elementGraphType = _graphTypeProvider.GetGraphType(graphType.Type, false, false);
                var listGraphType = new ListGraphType(Convert(elementGraphType));
                return graphType.IsRequired
                    ? new NonNullGraphType(listGraphType)
                    : (IGraphTypeOfGraphQLNet)listGraphType;
            }

            //Scalar
            if (!graphType.Fields.Any())
            {
                if (graphType.IsEnum)
                {
                    var enumGraphType = typeof(EnumerationGraphType<>).MakeGenericType(graphType.Type);
                    enumGraphType = graphType.IsRequired
                        ? typeof(NonNullGraphType<>).MakeGenericType(enumGraphType)
                        : enumGraphType;
                    return (IGraphTypeOfGraphQLNet)Activator.CreateInstance(enumGraphType);
                }

                var scalarType = !graphType.IsRequired
                   ? GraphTypeTypeRegistry.Get(graphType.Type.GenericTypeArguments[0])
                   : GraphTypeTypeRegistry.Get(graphType.Type);

                if (null != scalarType)
                {
                    return graphType.IsRequired
                        ? (IGraphTypeOfGraphQLNet)Activator.CreateInstance(  typeof(NonNullGraphType<>).MakeGenericType(scalarType))
                        : (IGraphTypeOfGraphQLNet)Activator.CreateInstance(scalarType);
                }

                throw new GraphException($"Unknown GraphType '{graphType.Name}'");
            }

            //Complex
            var objectGraphType = new ObjectGraphType();
            foreach (var field in graphType.Fields.Values)
            {
                var fieldType = new FieldType
                {
                    Name = field.Name,
                    ResolvedType = Convert(field.GraphType)
                };

                foreach (var argument in field.Arguments.Values)
                {
                    var queryArgument = new QueryArgument(Convert(argument.GraphType)) { Name = argument.Name };
                    fieldType.Arguments.Add(queryArgument);
                }
                objectGraphType.AddField(fieldType);
            }

            return graphType.IsRequired
                ? (IGraphTypeOfGraphQLNet)new NonNullGraphType(objectGraphType)
                : objectGraphType;
        }
    }
}