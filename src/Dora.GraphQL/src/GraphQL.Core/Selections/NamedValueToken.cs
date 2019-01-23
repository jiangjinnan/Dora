namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents Name + Value structure.
    /// </summary>
    public struct NamedValueToken
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the value token.
        /// </summary>
        /// <value>
        /// The value token.
        /// </value>
        public object ValueToken { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is varible reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is varible reference; otherwise, <c>false</c>.
        /// </value>
        public bool IsVaribleReference { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueToken"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="valueToken">The value token.</param>
        /// <param name="isVariableReference">if set to <c>true</c> [is variable reference].</param>
        public NamedValueToken(string name, object valueToken, bool isVariableReference) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            ValueToken = Guard.ArgumentNotNull(valueToken, nameof(valueToken));
            IsVaribleReference = isVariableReference;
        }
    }
}
