using Dora.GraphQL.Selections;

namespace Dora.GraphQL
{
    public static class SelectionNodeExtensions
    {
        public static ISelectionNode AddArgument(this ISelectionNode selection, NamedValueToken argument)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            selection.Arguments[argument.Name] = argument;
            return selection;
        }

        public static ISelectionNode AddDirective(this ISelectionNode selection, IDirective  directive)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            selection.Directives[directive.Name] = directive;
            return selection;
        }

        public static ISelectionNode AddChild(this ISelectionNode selection, ISelectionNode child)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            Guard.ArgumentNotNull(child, nameof(child));
            selection.Children[child.Name] = child;
            return selection;
        }
    }
}
