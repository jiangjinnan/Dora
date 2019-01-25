using Dora.GraphQL.GraphTypes;

namespace Dora.GraphQL.Schemas
{
    /// <summary>
    /// Represents application's GraphQL schema.
    /// </summary>
    public interface IGraphSchema: IGraphType
    {
        /// <summary>
        /// Gets the query specific <see cref="IGraphType"/>.
        /// </summary>
        /// <value>
        /// The query specific <see cref="IGraphType"/>.
        /// </value>
        IGraphType Query { get; }

        /// <summary>
        /// Gets the mutation specific <see cref="IGraphType"/>.
        /// </summary>
        /// <value>
        /// The mutation specific <see cref="IGraphType"/>.
        /// </value>
        IGraphType Mutation { get; }

        /// <summary>
        /// Gets the subsription specific <see cref="IGraphType"/>.
        /// </summary>
        /// <value>
        /// The subsription specific <see cref="IGraphType"/>.
        /// </value>
        IGraphType Subscription { get; }
    }
}
