namespace Dora.GraphQL
{
    /// <summary>
    /// Represents GraphQL operation type.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Query
        /// </summary>
        Query = 0,

        /// <summary>
        /// Mutation
        /// </summary>
        Mutation = 1,

        /// <summary>
        /// Subscription
        /// </summary>
        Subscription = 2
    }
}
