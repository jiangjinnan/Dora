using Dora.GraphQL.Schemas;

namespace Dora.GraphQL
{
    /// <summary>
    /// <see cref="GraphOperationAttribute"/> for query operation type.
    /// </summary>
    public sealed class MutationAttribute : GraphOperationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute"/> class.
        /// </summary>
        public MutationAttribute() : base(OperationType.Mutation) { }
    }
}
