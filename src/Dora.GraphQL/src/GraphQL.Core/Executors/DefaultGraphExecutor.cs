using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.GraphQL.Executors
{
    public class DefaultGraphExecutor : IGraphExecutor
    {
        public async  ValueTask<ExecutionResult> ExecuteAsync(GraphContext graphContext)
        {
            var selections = graphContext.SelectionSet;
            if (selections.Count == 1)
            {
                var selection = selections.Single().Value;
                var resoloverContext = new ResolverContext(graphContext, graphContext.Operation, selection, null);
                var result = await ExecuteCoreAsync(graphContext, resoloverContext, graphContext.Operation, selection);
                return new ExecutionResult { Data = result };
            }

            var dictionary = new Dictionary<string, object>();
            foreach (var subSelection in graphContext.SelectionSet.Values)
            {
                var resoloverContext = new ResolverContext(graphContext, graphContext.Operation, subSelection, null);
                var result = await ExecuteCoreAsync(graphContext, resoloverContext, graphContext.Operation, subSelection);
                dictionary[subSelection.Alias ?? subSelection.Name] = result;
            }

            return new ExecutionResult { Data = dictionary };
        }

        private async ValueTask<object> ExecuteCoreAsync(GraphContext graphContext, ResolverContext resolverContext, GraphField field, ISelectionNode selection)
        {
            object container = await field.Resolver.ResolveAsync(resolverContext);
            var subSelections = string.IsNullOrEmpty(selection.Fragment)                
                ? selection.Children.Values
                : graphContext.Fragments[selection.Fragment].SelectionSet;
            if (!subSelections.Any())
            {
                return container;
            }            

            if (field.GraphType.IsEnumerable)
            {
                var list = new List<object>();
                foreach (var element in (IEnumerable)container)
                {
                    var node1 = new Dictionary<string, object>();
                    foreach (var subSelection in subSelections)
                    {
                        var subField = field.GraphType.Fields[subSelection.Name];
                        var newResolverContext = new ResolverContext(graphContext, subField, subSelection, element);
                        var element1 = await ExecuteCoreAsync(graphContext, newResolverContext, subField, subSelection);
                        node1[subSelection.Alias ?? subSelection.Name] = element1;
                    }
                    list.Add(node1);
                }
                return list;
            }

            var node = new Dictionary<string, object>();
            foreach (var subSelection in subSelections)
            {
                var subField = field.GraphType.Fields[subSelection.Name];
                var newResolverContext = new ResolverContext(graphContext,  subField, subSelection, container);
                var element = await ExecuteCoreAsync(graphContext,newResolverContext, subField, subSelection);
                node[subSelection.Alias??subSelection.Name] = element;
            }
            return node;
        }
    }
}
