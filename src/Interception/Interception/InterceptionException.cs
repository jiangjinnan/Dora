namespace Dora.Interception
{
    /// <summary>
    /// Interception based exception.
    /// </summary>
    [Serializable]
    public class InterceptionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionException"/> class.
        /// </summary>
        public InterceptionException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InterceptionException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public InterceptionException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InterceptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}