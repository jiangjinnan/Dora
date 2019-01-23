namespace Dora.GraphQL
{
    /// <summary>
    /// <see cref="GraphOperationAttribute"/> for query operation type.
    /// </summary>
    public sealed class SubscriptionAttribute : GraphOperationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute"/> class.
        /// </summary>
        public SubscriptionAttribute() : base(OperationType.Subscription) { }
    }
}
