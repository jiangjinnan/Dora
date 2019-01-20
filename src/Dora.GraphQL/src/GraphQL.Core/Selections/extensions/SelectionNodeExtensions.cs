using Dora.GraphQL.Selections;

namespace Dora.GraphQL
{
    public static class SelectionNodeExtensions
    {
        public static ISelectionNode AddArgument(this IFieldSelection selection, NamedValueToken argument)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            selection.Arguments[argument.Name] = argument;
            return selection;
        }

        public static ISelectionNode AddDirective(this IFieldSelection selection, IDirective  directive)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            selection.Directives[directive.Name] = directive;
            return selection;
        }

        public static ISelectionNode AddSubSelection(this ISelectionNode selection, ISelectionNode child)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            Guard.ArgumentNotNull(child, nameof(child));
            selection.SelectionSet.Add(child);
            return selection;
        }
    }
}
