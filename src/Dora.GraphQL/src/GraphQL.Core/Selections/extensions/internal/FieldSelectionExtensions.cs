using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;
using System.Linq;

namespace Dora.GraphQL
{
    internal static class FieldSelectionExtensions
    {
        public static bool IncludeAllFields(this IFieldSelection field)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            return field.Properties.TryGetValue(GraphDefaults.PropertyNames.IncludeAllFields, out var value)
                ? (bool)value
                : false;
        }

        public static bool IsSubQueryTree(this IFieldSelection field)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            return field.Properties.TryGetValue(GraphDefaults.PropertyNames.IsSubQueryTree, out var value)
                ? (bool)value
                : false;
        }

        public static bool TryGetQueryResultType(this  IFieldSelection field, out Type type)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            if (field.Properties.TryGetValue(GraphDefaults.PropertyNames.QueryResultType, out var value))
            {
                type = (Type)value;
                return true;
            }
            return (type = null) != null;
        }

        public static bool SetIncludeAllFieldsFlags(
            this IFieldSelection selection, 
            GraphField graphField, 
            IQueryResultTypeGenerator typeGenerator, 
            ref bool generateQueryResultType,
            out bool isSubQueryTree)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            Guard.ArgumentNotNull(graphField, nameof(graphField));
            isSubQueryTree = true;

            bool? flags = null;
            if (graphField.HasCustomResolver() || selection.Directives.Any() || selection.SelectionSet.Any(it=>it is IFragment))
            {
                generateQueryResultType = false;
                isSubQueryTree = false;
                flags = false;
            }

            var subFieldSelections = selection.SelectionSet.OfType<IFieldSelection>().ToArray();
            foreach (var subSelection in subFieldSelections)
            {
                var subFields = graphField.GraphType.Fields.Values.Where(it => it.Name == subSelection.Name).ToArray();

                if (subFields.Length > 1)
                {
                    throw new GraphException($"{graphField.GraphType.Name} is a union GraphType, please perform query using fragement.");
                }
                if (subFields.Length == 0)
                {
                    throw new GraphException($"Field '{subSelection.Name}' is not defined in the GraphType '{graphField.GraphType.Name}'");
                }
                if (!SetIncludeAllFieldsFlags(subSelection, subFields[0], typeGenerator, ref generateQueryResultType, out var isSubtree))
                {
                    flags = false;
                    if (!isSubtree)
                    {
                        isSubQueryTree = false;
                    }
                }
            }

            var fragements = selection.SelectionSet.OfType<IFragment>().ToArray();
            foreach (var fragement in fragements)
            {
                foreach (IFieldSelection fieldSelection in fragement.SelectionSet)
                {
                    var fields = fragement.GraphType.Fields.Values.Where(it => it.Name == fieldSelection.Name).ToArray();
                    if (fields.Length > 1)
                    {
                        throw new GraphException($"{graphField.GraphType.Name} is a union GraphType, please perform query using fragement.");
                    }
                    if (fields.Length == 0)
                    {
                        throw new GraphException($"Field '{fieldSelection.Name}' is not defined in the GraphType '{fragement.GraphType.Name}'");
                    }

                    if (!SetIncludeAllFieldsFlags(fieldSelection, fields[0], typeGenerator, ref generateQueryResultType, out var isSubtree))
                    {
                        flags = false;
                        if (!isSubtree)
                        {
                            isSubQueryTree = false;
                        }
                    }
                }
            }

            if (isSubQueryTree == false)
            {
                selection.Properties[GraphDefaults.PropertyNames.IsSubQueryTree] = false;
            }
            else
            {
                if (graphField.GraphType.Fields.Any() && generateQueryResultType)
                {
                    selection.Properties[GraphDefaults.PropertyNames.QueryResultType] = typeGenerator.Generate(selection, graphField);
                    selection.Properties[GraphDefaults.PropertyNames.IsSubQueryTree] = false;
                }
            }

            if (flags == false)
            {
                selection.Properties[GraphDefaults.PropertyNames.IncludeAllFields] = false;
                return false;
            }

            if (IncludeAllMembers(selection, graphField))
            {
                selection.Properties[GraphDefaults.PropertyNames.IncludeAllFields] = true;
                return true;
            }

            selection.Properties[GraphDefaults.PropertyNames.IncludeAllFields] = false;
            return false;
        }

        private static bool IncludeAllMembers(IFieldSelection fieldSelection, GraphField field)
        {
            var fieldNames = field.GraphType.Fields.Values.Select(it=>it.Name).Distinct().ToArray();
            var selectedFieldNames = fieldSelection.SelectionSet.OfType<IFieldSelection>().Select(it => it.Name).Distinct().ToArray();
            var invalidFieldNames = selectedFieldNames.Except(fieldNames);
            if (invalidFieldNames.Any())
            {
                throw new GraphException($"Specified field(s) '{string.Join(", ", invalidFieldNames)}' is/are not defined in the GraphType '{field.GraphType.Name}'");
            }

            return fieldNames.Length == selectedFieldNames.Length;
        }
    }
}
