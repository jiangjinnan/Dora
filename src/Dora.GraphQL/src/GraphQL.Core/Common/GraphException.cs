
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class GraphException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        public GraphException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GraphException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public GraphException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected GraphException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
