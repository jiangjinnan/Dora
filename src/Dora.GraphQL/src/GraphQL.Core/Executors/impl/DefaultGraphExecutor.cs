using AutoMapper;
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
        private readonly IGraphTypeProvider _graphTypeProvider;
        public DefaultGraphExecutor(IGraphTypeProvider graphTypeProvider)
        {
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
        }

        public async  ValueTask<ExecutionResult> ExecuteAsync(GraphContext graphContext)
        {
            var selections = graphContext.SelectionSet;
            if (selections.Count == 1)
            {
                var selection = selections.OfType<IFieldSelection>().Single();
                var resoloverContext = new ResolverContext(graphContext, graphContext.Operation, selection, null);
                var result = await ExecuteCoreAsync(graphContext, resoloverContext, graphContext.Operation, selection);
                return new ExecutionResult { Data = result };
            }

            var dictionary = new Dictionary<string, object>();
            foreach (IFieldSelection subSelection in graphContext.SelectionSet)
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
            if (selection is IFieldSelection fieldSelection1)
            {
                if (fieldSelection1.IncludeAllFields())
                {
                    return container;
                }

                if (fieldSelection1.TryGetQueryResultType(out var queryResultType))
                {
                    return Mapper.Map(container, container.GetType(), queryResultType);
                }
            }


            Console.WriteLine("Missing...");

            var subFields = new List<IFieldSelection>();
            subFields.AddRange(selection.SelectionSet.OfType<IFieldSelection>());                    

            if (field.GraphType.IsEnumerable)
            {
                var list = new List<object>();
                foreach (var element in (IEnumerable)container)
                {
                    List<IFieldSelection> elementSubFields = null;
                    if (TryGetFragment(element, selection, out var fragment1))
                    {
                        elementSubFields = new List<IFieldSelection>(subFields);
                        elementSubFields.AddRange(fragment1.SelectionSet.OfType<IFieldSelection>());
                    }
                    var allFields = elementSubFields ?? subFields;
                    if (!allFields.Any())
                    {
                        list.Add(element);
                        continue;
                    }                   
                    var dictionary = new Dictionary<string, object>();
                    foreach (var fieldSelection in allFields)
                    {
                        field.GraphType.Fields.TryGetGetField(element, fieldSelection.Name, out var subField);
                        var newResolverContext = new ResolverContext(graphContext, subField, fieldSelection, element);
                        if (newResolverContext.Skip())
                        {
                            continue;
                        }
                        var element1 = await ExecuteCoreAsync(graphContext, newResolverContext, subField, fieldSelection);
                        dictionary[fieldSelection.Alias ?? fieldSelection.Name] = element1;
                    }
                    list.Add(dictionary);
                }
                return list;
            }

            if (TryGetFragment(resolverContext, selection, out var fragment2))
            {
                subFields.AddRange(fragment2.SelectionSet.OfType<IFieldSelection>());
            }
            if (!subFields.Any())
            {
                return container;
            }
            var node = new Dictionary<string, object>();
            foreach (IFieldSelection subSelection in subFields)
            {
                field.GraphType.Fields.TryGetGetField(container, subSelection.Name, out var subField);
                var newResolverContext = new ResolverContext(graphContext,  subField, subSelection, container);
                if (newResolverContext.Skip())
                {
                    continue;
                }
                var element = await ExecuteCoreAsync(graphContext,newResolverContext, subField, subSelection);
                node[subSelection.Alias??subSelection.Name] = element;
            }
            return node;
        }

        private bool TryGetFragment(object container, ISelectionNode selection, out IFragment fragment)
        {
            var fragments = selection.SelectionSet.OfType<IFragment>().ToDictionary(it=>it.GraphType, it=>it);
            if (fragments.Count == 0 || null == container)
            {
                return (fragment = null) != null;
            }
            var containerType = container.GetType();
            var graphTypes = fragments.Select(it => it.Key).ToArray();
            var graphType = SelectBest(containerType);
            if (null == graphType)
            {
                return (fragment = null) != null;
            }

            return (fragment = fragments[graphType]) != null;

            IGraphType SelectBest(Type type)
            {
                var best = graphTypes.SingleOrDefault(it => it.Type == type );
                if (null != best)
                {
                    return best;
                }

                var parent = type.BaseType;
                return parent == null
                    ? null
                    : SelectBest(parent);
            }
        }
    }
}
