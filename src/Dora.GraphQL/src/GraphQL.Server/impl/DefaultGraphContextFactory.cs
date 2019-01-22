using Dora.GraphQL;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using GraphQL.Execution;
using GraphQL.Language.AST;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NamedType = GraphQL.Language.AST.NamedType;
using OperationType = Dora.GraphQL.Schemas.OperationType;

namespace Dora.GraphQL.Server
{
    public class DefaultGraphContextFactory : IGraphContextFactory
    {
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IGraphSchemaProvider  _schemaProvider;
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISelectionSetProvider _selectionSetProvider;

        public DefaultGraphContextFactory(
           IDocumentBuilder documentBuilder,
           IGraphSchemaProvider schemaProvider, 
           IGraphTypeProvider graphTypeProvider,
           IHttpContextAccessor httpContextAccessor,
           ISelectionSetProvider selectionSetProvider)
        {
            _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _selectionSetProvider = selectionSetProvider ?? throw new ArgumentNullException(nameof(selectionSetProvider));
        }

        public ValueTask<GraphContext> CreateAsync(RequestPayload payload)
        {
            var document = _documentBuilder.Build(payload.Query);
            var operation = document.Operations.Single();
            var operationName = operation.Name;
            var operationType = (OperationType)(int)operation.OperationType;
            IGraphType graphType;
            var graphSchema = _schemaProvider.GetSchema();
            switch (operationType)
            {
                case OperationType.Query:
                    {
                        graphType = graphSchema.Query;
                        break;
                    }
                case OperationType.Mutation:
                    {
                        graphType = graphSchema.Mutation;
                        break;
                    }
                default:
                    {
                        graphType = graphSchema.Subsription;
                        break;
                    }
            }

            if (!graphType.Fields.TryGetGetField(typeof(void),operationName, out var operationField))
            {
                throw new GraphException($"Specified GraphQL operation '{operationName}' does not exist in the schema");
            }
            var context = new GraphContext(operationName, operationType, operationField, _httpContextAccessor.HttpContext.RequestServices);
            SetArguments(context, operation);
            context.SelectionSet = _selectionSetProvider.GetSelectionSet(payload.Query, operation, document.Fragments);
            SetVariables(context, payload);
            context.SetArguments(payload.Query);
            return new ValueTask<GraphContext>(context);
        }

        private void SetArguments(GraphContext context, Operation operation)
        {
            foreach (var variable in operation.Variables)
            {
                var name = variable.Name;
                var graphTypeName = "";
                var namedType = variable.Type as NamedType;
                if (namedType != null)
                {
                    graphTypeName = namedType.Name;
                }
                else
                {
                    var notNullType = variable.Type as NonNullType;
                    graphTypeName = $"{((NamedType)notNullType.Type).Name}!";
                }

                if (string.IsNullOrWhiteSpace(graphTypeName))
                {
                    throw new GraphException($"Does not specify the GraphType for the argument '{variable.Name}'");
                }

                if (!_graphTypeProvider.TryGetGraphType(graphTypeName, out var graphType))
                {
                    throw new GraphException($"Cannot resolve the GraphQL type '{graphTypeName}'");
                }                
                var defaultValue = variable.DefaultValue == null
                    ? null
                    : variable.DefaultValue.Value;
                context.AddArgument(new NamedGraphType(name, graphType, defaultValue));
            }
        }

        private void SetVariables(GraphContext context, RequestPayload payload)
        {
            if (Extensions.GetValue(payload.Variables) is Dictionary<string, object> dictionary)
            {
                foreach (var item in dictionary)
                {
                    context.Variables[item.Key] = item.Value;
                }
            }
        }

    }

    internal static class Extensions
    {
        public static object GetValue(this object value)
        {
            if (value is JObject obj2)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                foreach (KeyValuePair<string, JToken> pair in obj2)
                {
                    string introduced9 = pair.Key;
                    dictionary.Add(introduced9, pair.Value.GetValue());
                }
                return dictionary;
            }
            if (value is JProperty property)
            {
                return new Dictionary<string, object> {
                    {
                        property.Name,
                        property.Value.GetValue()
                    }
                };
            }
            if (value is JArray array)
            {
                return Enumerable.Aggregate(array.Children(), new List<object>(), delegate (List<object> list, JToken token)
                {
                    list.Add(token.GetValue());
                    return list;
                });
            }
            if (!(value is JValue value2))
            {
                return value;
            }
            object obj3 = value2.Value;
            if (obj3 is long)
            {
                long num = (long)obj3;
                if ((num >= -2147483648L) && (num <= 0x7fffffffL))
                {
                    return (int)num;
                }
            }
            return obj3;
        }
    }
}
