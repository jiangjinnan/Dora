using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Selections;
using GraphQL.Language.AST;
using Microsoft.Extensions.Options;
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
        private readonly IGraphSchema   _schema;
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly ConcurrentDictionary<string, ICollection<ISelectionNode>> _selections;
        private readonly IQueryResultTypeGenerator _typeGenerator;
        private readonly FieldNameNormalizer _nameNormalizer;

        public SelectionSetProvider(
            IGraphTypeProvider graphTypeProvider, 
            IGraphSchemaProvider schemaProvider,
            IQueryResultTypeGenerator typeGenerator,
            IOptions<GraphServerOptions> optionsAccessor)
        {
            Guard.ArgumentNotNull(schemaProvider, nameof(schemaProvider));
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _selections = new ConcurrentDictionary<string, ICollection<ISelectionNode>>();
            _typeGenerator = Guard.ArgumentNotNull(typeGenerator, nameof(typeGenerator));
            _schema = schemaProvider.GetSchema();
            _nameNormalizer = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.FieldNamingConvention == Options.FieldNamingConvention.PascalCase
                ? FieldNameNormalizer.PascalCase
                : FieldNameNormalizer.CamelCase;
        }

        public ICollection<ISelectionNode> GetSelectionSet(string query, Operation operation, Fragments fragments)
        {
            Guard.ArgumentNotNullOrWhiteSpace(query, nameof(query));
            Guard.ArgumentNotNull(operation, nameof(operation));

            var key = query.TryNormalizeQuery(out var normalized, out _)
                ? normalized
                : query;

            if (_selections.TryGetValue(key, out var value))
            {
                return value;
            }

            var selections = CreateSelections(query, operation, fragments);
            if (selections.Count > 1)
            {
                return selections;
            }

            foreach (var subField in selections.Single().SelectionSet.OfType<IFieldSelection>())
            {
                if (HasArguments(subField))
                {
                    return selections;
                }
            }

            _selections.TryAdd(key, selections);
            return selections;
        }       

        private bool HasArguments(IFieldSelection selection)
        {
            if (selection.Arguments.Values.Any(it => !it.IsVaribleReference))
            {
                return true;
            }

            foreach (var subField in selection.SelectionSet.OfType<IFieldSelection>())
            {
                if (HasArguments(subField))
                {
                    return true;
                }
            }

            return false;
        }

        private ICollection<ISelectionNode> CreateSelections(string query, Operation operation, Fragments fragments)
        {
            var dictionary = new Dictionary<string, IFragment>();
            SetFragments(fragments, dictionary);
            var selections = ResolveSelections(operation.SelectionSet.Selections, dictionary);

            IGraphType graphType = _schema.Fields.Values.Single(it => it.Name == operation.OperationType.ToString()).GraphType;
            foreach (var fieldSelection in selections.OfType<IFieldSelection>())
            {
                var field = graphType.Fields.Values.Single(it => it.Name == operation.Name);
                fieldSelection.SetIncludeAllFieldsFlags(field, _typeGenerator, out var isSubQueryTree);
            }
            return selections;
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
                var fieldSelection = new FieldSelection(_nameNormalizer.NormalizeFromSource(field.Name))
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
                    fieldSelection.Directives.Add(directive);
                }

                foreach (var argument in field.Arguments)
                {
                    fieldSelection.AddArgument(new NamedValueToken(argument.Name, argument.Value.Value, argument.Value is VariableReference));
                }

                var subSelections = ResolveSelections(field.SelectionSet.Selections, fragements);
                foreach (var subSelection in subSelections)
                {
                    fieldSelection.AddSubSelection(subSelection);
                }
                list.Add(fieldSelection);
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
