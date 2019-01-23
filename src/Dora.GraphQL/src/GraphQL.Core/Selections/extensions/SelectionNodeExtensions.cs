using Dora.GraphQL.Selections;

namespace Dora.GraphQL
{
    /// <summary>
    /// Defines <see cref="IFieldSelection"/> specific extension methods.
    /// </summary>
    public static class SelectionNodeExtensions
    {
        /// <summary>
        /// Adds the argument to specified <see cref="IFieldSelection"/>.
        /// </summary>
        /// <param name="field">The <see cref="IFieldSelection"/> to which the argument is added.</param>
        /// <param name="argument">The argument.</param>
        /// <returns>The <see cref="IFieldSelection"/>.</returns>
        public static IFieldSelection AddArgument(this IFieldSelection field, NamedValueToken argument)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            field.Arguments[argument.Name] = argument;
            return field;
        }

        /// <summary>
        /// Adds the sub selection.
        /// </summary>
        /// <param name="selection">The <see cref="ISelectionNode"/> to which the sub selection is added.</param>
        /// <param name="child">The sub selection.</param>
        /// <returns>The <see cref="ISelectionNode"/></returns>
        public static ISelectionNode AddSubSelection(this ISelectionNode selection, ISelectionNode child)
        {
            Guard.ArgumentNotNull(selection, nameof(selection));
            Guard.ArgumentNotNull(child, nameof(child));
            selection.SelectionSet.Add(child);
            return selection;
        }
    }
}