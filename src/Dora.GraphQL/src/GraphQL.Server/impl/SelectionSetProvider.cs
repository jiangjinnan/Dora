using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using GraphQL.Language.AST;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Directive = Dora.GraphQL.Selections.Directive;
using IFragment = Dora.GraphQL.Selections.IFragment;

namespace Dora.GraphQL.Server
{
    public class SelectionSetProvider : ISelectionSetProvider
    {
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly ConcurrentDictionary<string, ICollection<ISelectionNode>> _selections;

        public SelectionSetProvider(IGraphTypeProvider graphTypeProvider)
        {
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _selections = new ConcurrentDictionary<string, ICollection<ISelectionNode>>();
        }

        public ICollection<ISelectionNode> GetSelectionSet(string query, Operation operation, Fragments fragments)
        {
            Guard.ArgumentNotNullOrWhiteSpace(query, nameof(query));
            Guard.ArgumentNotNull(operation, nameof(operation));
            return _selections.GetOrAdd(query, _ =>
            {
                var dictionary = new Dictionary<string, IFragment>();
                SetFragments(fragments, dictionary);
                return ResolveSelections(operation.SelectionSet.Selections, dictionary);
            });
        }
        private ICollection<ISelectionNode> ResolveSelections(IEnumerable<ISelection> selections, Dictionary<string, IFragment> fragements)
        {
            var list = new List<ISelectionNode>();
            foreach (var selection in selections)
            {
                if (selection is InlineFragment inlineFragment)
                {
                    var graphType = _graphTypeProvider.GetGraphType(inlineFragment.Type.Name);
                    var fragment = new Fragment(graphType);
                    foreach (var item in ResolveSelections(inlineFragment.SelectionSet.Selections, fragements))
                    {
                        fragment.AddSubSelection(item);
                    }
                    list.Add(fragment);
                }
                if (!(selection is Field field))
                {
                    continue;
                }
                var selectionNode = new FieldSelection(field.Name)
                {
                    Alias = field.Alias
                };

                if (field.SelectionSet.Selections.FirstOrDefault() is FragmentSpread fragmentSpread)
                {
                    var fragmentType = fragements[fragmentSpread.Name];
                }

                foreach (var directiveDefinition in field.Directives)
                {
                    var directive = new Directive(directiveDefinition.Name);
                    foreach (var argument in directiveDefinition.Arguments)
                    {
                        directive.AddArgument(new NamedValueToken(argument.Name, argument.Value.Value, argument.Value is VariableReference));
                    }
                    selectionNode.Directives.Add(directive);
                }

                foreach (var argument in field.Arguments)
                {
                    selectionNode.AddArgument(new NamedValueToken(argument.Name, argument.Value.Value, argument.Value is VariableReference));
                }

                var subSelections = ResolveSelections(field.SelectionSet.Selections, fragements);
                foreach (var subSelection in subSelections)
                {
                    selectionNode.AddSubSelection(subSelection);
                }
                list.Add(selectionNode);
            }
            return list;
        }

        private void SetFragments(Fragments fragments, Dictionary<string, IFragment> dictionary)
        {
            foreach (var definition in fragments)
            {
                var graphTypeName = definition.Type.Name;
                var graphType = _graphTypeProvider.GetGraphType(graphTypeName);
                var fragement = new Fragment(graphType);
                foreach (var selection in ResolveSelections(definition.SelectionSet.Selections, dictionary))
                {
                    fragement.SelectionSet.Add(selection);
                }
                dictionary.Add(definition.Name, fragement);
            }
        }
    }
}
