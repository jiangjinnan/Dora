namespace Dora.Interception
{
    /// <summary>
    /// Wrapper of specified type of item for sorting.
    /// </summary>
    public sealed class Sortable<T>
    {
        /// <summary>
        /// Gets the order used for sorting.
        /// </summary>
        /// <value>
        /// The order used for sorting..
        /// </value>
        public int Order { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sortable{T}"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="value">The value.</param>
        public Sortable(int order, T value)
        {
            Order = order;
            Value = value;
        }
    }
}
