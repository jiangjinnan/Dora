using Dora.GraphQL;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL;
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
            if (graphSchema.Query.Fields.Any())
            {
                schema.Query = CreateSchema(OperationType.Query, graphSchema.Query);
            }
            if (graphSchema.Mutation.Fields.Any())
            {
                schema.Mutation = CreateSchema(OperationType.Mutation, graphSchema.Mutation, schema);
            }
            if (graphSchema.Subscription.Fields.Any())
            {
                schema.Subscription = CreateSchema(OperationType.Subscription, graphSchema.Subscription);
            }

            return schema;
        }
        private IGraphTypeOfGraphQLNet Convert(IGraphType graphType, bool input)
        {
            try
            {
                //Enumerable
                if (graphType.IsEnumerable)
                {
                    var elementGraphType = _graphTypeProvider.GetGraphType(graphType.Type, false, false);
                    var listGraphType = new ListGraphType(Convert(elementGraphType, input));
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

                    if (graphType.Type == typeof(string))
                    {
                        var stringType = new StringGraphType();
                        return graphType.IsRequired
                            ? (IGraphTypeOfGraphQLNet)new NonNullGraphType(stringType)
                            : stringType;
                    }
                    var scalarType = graphType.Type.IsGenericType && graphType.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
                       ? GraphTypeTypeRegistry.Get(graphType.Type.GenericTypeArguments[0])
                       : GraphTypeTypeRegistry.Get(graphType.Type);

                    if (null != scalarType)
                    {
                        return graphType.IsRequired
                            ? (IGraphTypeOfGraphQLNet)Activator.CreateInstance(typeof(NonNullGraphType<>).MakeGenericType(scalarType))
                            : (IGraphTypeOfGraphQLNet)Activator.CreateInstance(scalarType);
                    }

                    throw new GraphException($"Unknown GraphType '{graphType.Name}'");
                }

                //Complex
                var objectGraphType = input
                    ? (IComplexGraphType)new InputObjectGraphType { Name = $"{graphType.Name}Input"}
                    : new ObjectGraphType { Name = graphType.Name };
                foreach (var field in graphType.Fields.Values)
                {
                    var fieldType = new FieldType
                    {
                        Name = field.Name,
                        ResolvedType = Convert(field.GraphType, input)
                    };
                    if (field.Arguments.Any())
                    {
                        fieldType.Arguments = new QueryArguments();
                    }
                    foreach (var argument in field.Arguments.Values)
                    {
                        var queryArgument = new QueryArgument(Convert(argument.GraphType, input)) { Name = argument.Name };
                        fieldType.Arguments.Add(queryArgument);
                    }
                    objectGraphType.AddField(fieldType);
                }

                return graphType.IsRequired
                    ? (IGraphTypeOfGraphQLNet)new NonNullGraphType(objectGraphType)
                    : objectGraphType;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private ObjectGraphType CreateSchema(OperationType operationType, IGraphType graphType, GraphQLNetSchema  root = null)
        {
            ObjectGraphType schema;
            switch (operationType)
            {
                case OperationType.Query:
                    {
                        schema = new QuerySchema();
                        break;
                    }
                case OperationType.Mutation:
                    {
                        schema = new MutationSchema();
                        break;
                    }
                    default:
                    {
                        schema = new SubscruptionSchema();
                        break;
                    }
            }

            foreach (var field in graphType.Fields.Values)
            {
                var fieldType = new FieldType
                {
                    Name = field.Name,
                    ResolvedType = Convert(field.GraphType, false)
                };
                if (field.Arguments.Any())
                {
                    fieldType.Arguments = new QueryArguments();
                }
                foreach (var argument in field.Arguments.Values)
                {
                    var argumentType = Convert(argument.GraphType, operationType == OperationType.Mutation);
                    root?.RegisterType(argumentType);
                    var queryArgument = new QueryArgument(argumentType) { Name = argument.Name };
                    fieldType.Arguments.Add(queryArgument);
                }
                schema.AddField(fieldType);
            }
            return schema;
        }
        private class QuerySchema : ObjectGraphType { }
        private class MutationSchema : ObjectGraphType { }
        private class SubscruptionSchema : ObjectGraphType { }
    }
}