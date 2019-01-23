namespace Dora.GraphQL
{
    /// <summary>
    /// <see cref="GraphOperationAttribute"/> for query operation type.
    /// </summary>
    public sealed class QueryAttribute : GraphOperationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class.
        /// </summary>
        public QueryAttribute() : base(OperationType.Query) { }
    }
}
