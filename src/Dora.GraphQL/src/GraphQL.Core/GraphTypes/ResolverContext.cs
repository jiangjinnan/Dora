using Dora.GraphQL.Executors;
using Dora.GraphQL.Selections;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represents the <see cref="IGraphResolver"/> specific execution context.
    /// </summary>
    public struct ResolverContext
    {
        /// <summary>
        /// Gets the current <see cref="GraphContext"/>.
        /// </summary>
        /// <value>
        /// The current <see cref="GraphContext"/>.
        /// </value>
        public GraphContext GraphContext { get; }

        /// <summary>
        /// Gets current selection field specific <see cref="GraphField"/>.
        /// </summary>
        /// <value>
        /// The current selection field specific <see cref="GraphField"/>.
        /// </value>
        public GraphField  Field { get; }

        /// <summary>
        /// Gets the selection field.
        /// </summary>
        /// <value>
        /// The selection field.
        /// </value>
        public IFieldSelection Selection { get; }

        /// <summary>
        /// Gets the container representing the value of parent selection node.
        /// </summary>
        /// <value>
        /// The container representing the value of parent selection node.
        /// </value>
        public object Container { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverContext"/> struct.
        /// </summary>
        /// <param name="graphContext">The graph context.</param>
        /// <param name="field">The current selection field specific <see cref="GraphField"/>.</param>
        /// <param name="selection">The selection field.</param>
        /// <param name="container">The container representing the value of parent selection node.</param>
        public ResolverContext(GraphContext graphContext, GraphField  field, IFieldSelection selection,  object container)
        {
            Container = container;
            GraphContext = Guard.ArgumentNotNull(graphContext, nameof(graphContext));
            Field = Guard.ArgumentNotNull(field, nameof(field));
            Selection = Guard.ArgumentNotNull(selection, nameof(selection));
        }
    }
}