using Dora.GraphQL;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using System;
using System.Collections.Generic;
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
        public ISchema Convert(IGraphSchema graphSchema)
        {
            var schema = new Schema();
            if (graphSchema.Query.Fields.Any())
            {
                schema.Query = CreateSchema(OperationType.Query, graphSchema.Query, schema);
            }
            if (graphSchema.Mutation.Fields.Any())
            {
                schema.Mutation = CreateSchema(OperationType.Mutation, graphSchema.Mutation, schema);
            }
            if (graphSchema.Subscription.Fields.Any())
            {
                schema.Subscription = CreateSchema(OperationType.Subscription, graphSchema.Subscription, schema);
            }

            return schema;
        }
        private IGraphTypeOfGraphQLNet Convert(IGraphType graphType, bool input, ISchema root)
        {
            try
            {
                //Union
                if (graphType.OtherTypes.Any() && !input)
                {
                    var list = new List<IObjectGraphType>();
                    var convertedGraphType = (IObjectGraphType)Convert(_graphTypeProvider.GetGraphType(graphType.Type, false, false), false, root);
                    convertedGraphType.IsTypeOf = _ => true;
                    root.RegisterType(convertedGraphType);
                    list.Add(convertedGraphType);
                    foreach (var type in graphType.OtherTypes)
                    {
                        convertedGraphType = (IObjectGraphType)Convert(_graphTypeProvider.GetGraphType(type, false, false), false, root);
                        convertedGraphType.IsTypeOf = _ => true;
                        root.RegisterType(convertedGraphType);
                        list.Add(convertedGraphType);
                    }
                    var unionType = new UnionGraphType { PossibleTypes = list };
                    root.RegisterType(unionType);
                    return unionType;
                }

                //Enumerable
                if (graphType.IsEnumerable)
                {
                    var elementGraphType = _graphTypeProvider.GetGraphType(graphType.Type, false, false);
                    var listGraphType = new ListGraphType(Convert(elementGraphType, input, root));
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
                        var resolvedGraphType = (IGraphTypeOfGraphQLNet)Activator.CreateInstance(scalarType);
                        return graphType.IsRequired
                            ? new NonNullGraphType(resolvedGraphType)
                            : resolvedGraphType;
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
                        ResolvedType = Convert(field.GraphType, input, root)
                    };
                    if (field.Arguments.Any())
                    {
                        fieldType.Arguments = new QueryArguments();
                    }
                    foreach (var argument in field.Arguments.Values)
                    {
                        var queryArgument = new QueryArgument(Convert(argument.GraphType, input, root)) { Name = argument.Name };
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

        private ObjectGraphType CreateSchema(OperationType operationType, IGraphType graphType, ISchema  root)
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
                    ResolvedType = Convert(field.GraphType, false, root)
                };
                if (field.Arguments.Any())
                {
                    fieldType.Arguments = new QueryArguments();
                }
                foreach (var argument in field.Arguments.Values)
                {
                    var argumentType = Convert(argument.GraphType, operationType == OperationType.Mutation, root);
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