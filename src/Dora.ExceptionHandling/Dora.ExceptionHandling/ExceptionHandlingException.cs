using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{    
    /// <summary>
    /// Represents the exception thrown during perform exception handling.
    /// </summary>
    public class ExceptionHandlingException : Exception
    {
        /// <summary>
        /// Create a new <see cref="ExceptionHandlingException"/>.
        /// </summary>
        public ExceptionHandlingException() { }

        /// <summary>
        /// Create a new <see cref="ExceptionHandlingException"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ExceptionHandlingException(string message) : base(message) { }

        /// <summary>
        /// Create a new <see cref="ExceptionHandlingException"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ExceptionHandlingException(string message, Exception inner) : base(message, inner) { }
    }
}
