namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Represents the argument binding result.
    /// </summary>
    public struct ArgumentBindingResult
    {
        /// <summary>
        /// Gets argument value bound.
        /// </summary>
        /// <value>
        /// The argument value bound.
        /// </value>
        public object Value { get; }


        /// <summary>
        /// Gets a value indicating whether argument is successfully bound.
        /// </summary>
        /// <value>
        ///   <c>true</c> if argument is successfully bound; otherwise, <c>false</c>.
        /// </value>
        public bool IsArgumentBound { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBindingResult"/> struct.
        /// </summary>
        /// <param name="value">The argument value.</param>
        /// <param name="isArgumentBound">if set to <c>true</c> [is argument bound].</param>
        public ArgumentBindingResult(object value, bool isArgumentBound) : this()
        {
            Value = value;
            IsArgumentBound = isArgumentBound;
        }

        /// <summary>
        /// Create a <see cref="ArgumentBindingResult"/> of failed status.
        /// </summary>
        /// <returns>The <see cref="ArgumentBindingResult"/> of failed status.</returns>
        public static ArgumentBindingResult Failed() => new ArgumentBindingResult(null, false);

        /// <summary>
        /// Create a <see cref="ArgumentBindingResult"/> of successful status.
        /// </summary>
        /// <returns>The <see cref="ArgumentBindingResult"/> of successful status.</returns>
        public static ArgumentBindingResult Success(object value) => new ArgumentBindingResult(value, true);
    }
}
