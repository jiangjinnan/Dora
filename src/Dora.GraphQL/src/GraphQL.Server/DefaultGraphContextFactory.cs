using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Selections;
using Dora.GraphQL.Selections.impl;
using GraphQL.Execution;
using GraphQL.Language.AST;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Directive = Dora.GraphQL.Selections.impl.Directive;
using OperationType = Dora.GraphQL.Schemas.OperationType;
using Dora.GraphQL;
using NamedType = GraphQL.Language.AST.NamedType;

namespace Dora.GraphQL.Server
{
    public class DefaultGraphContextFactory : IGraphContextFactory
    {
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IGraphSchemaProvider  _schemaProvider;
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultGraphContextFactory(
           IDocumentBuilder documentBuilder,
           IGraphSchemaProvider schemaProvider, 
           IGraphTypeProvider graphTypeProvider,
           IHttpContextAccessor httpContextAccessor)
        {
            _documentBuilder = documentBuilder ?? throw new ArgumentNullException(nameof(documentBuilder));
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
            SetFragments(context, document.Fragments);
            SetArguments(context, operation);
            SetSelections(context, operation);
            SetVariables(context, payload);
            return new ValueTask<GraphContext>(context);
        }

        private void SetFragments(GraphContext context, Fragments fragments)
        {
            foreach (var definition in fragments)
            {
                var graphTypeName = definition.Type.Name;
                var graphType = _graphTypeProvider.GetGraphType(graphTypeName);
                var fragement = new Fragment(graphType);
                foreach (var selection in ResolveSelections(context, definition.SelectionSet.Selections))
                {
                    fragement.SelectionSet.Add(selection);
                }
                context.AddFragment(definition.Name, fragement);
            }
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

        private void SetSelections(GraphContext context, Operation operation)
        {
            foreach (var selection in ResolveSelections(context, operation.SelectionSet.Selections))
            {
                context.SelectionSet.Add(selection);
            }
        }

        private void SetVariables(GraphContext context, RequestPayload payload)
        {
            var dictionary = Extensions.GetValue(payload.Variables) as Dictionary<string, object>;
            if (null != dictionary)
            {
                foreach (var item in dictionary)
                {
                    context.Variables[item.Key] = item.Value;
                }
            }
        }

        private ICollection<ISelectionNode> ResolveSelections(GraphContext context, IEnumerable<ISelection> selections)
        {
            var list = new List<ISelectionNode>();
            foreach (var selection in selections)
            {
                var inlineFragment = selection as InlineFragment;
                if (null != inlineFragment)
                {
                    var graphType = _graphTypeProvider.GetGraphType(inlineFragment.Type.Name);
                    var fragment = new Fragment(graphType);
                    foreach (var item in ResolveSelections(context, inlineFragment.SelectionSet.Selections))
                    {
                        fragment.AddSubSelection(item);
                    }
                    list.Add(fragment);
                }
                var field = selection as Field;
                if (null == field)
                {
                    continue;
                }
                var selectionNode = new FieldSelection(field.Name)
                {
                    Alias = field.Alias
                };

                var fragmentSpread = field.SelectionSet.Selections.FirstOrDefault() as FragmentSpread;
                if (null != fragmentSpread)
                {
                    var fragmentType = context.Fragments[fragmentSpread.Name];
                }

                foreach (var directiveDefinition in field.Directives)
                {
                    var directive = new Directive(directiveDefinition.Name);
                    foreach (var argument in directiveDefinition.Arguments)
                    {
                        directive.AddArgument(new  NamedValueToken(argument.Name, argument.Value.Value, argument.Value is VariableReference));
                    }
                    selectionNode.Directives.Add(directive);
                }

                foreach (var argument in field.Arguments)
                {
                    selectionNode.AddArgument(new NamedValueToken(argument.Name, argument.Value.Value, argument.Value is VariableReference));
                }

                var subSelections = ResolveSelections(context, field.SelectionSet.Selections);
                foreach (var subSelection in subSelections)
                {
                    selectionNode.AddSubSelection(subSelection);
                }
                list.Add(selectionNode);
            }
            return list;
        }
    }

    internal static class Extensions
    {
        public static object GetValue(this object value)
        {
            JObject obj2 = value as JObject;
            if (obj2 != null)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                foreach (KeyValuePair<string, JToken> pair in obj2)
                {
                    string introduced9 = pair.Key;
                    dictionary.Add(introduced9, pair.Value.GetValue());
                }
                return dictionary;
            }
            JProperty property = value as JProperty;
            if (property != null)
            {
                return new Dictionary<string, object> { {
            property.Name,
            property.Value.GetValue()
        } };
            }
            JArray array = value as JArray;
            if (array != null)
            {
                return Enumerable.Aggregate(array.Children(), new List<object>(), delegate (List<object> list, JToken token) {
                    list.Add(token.GetValue());
                    return list;
                });
            }
            JValue value2 = value as JValue;
            if (value2 == null)
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
