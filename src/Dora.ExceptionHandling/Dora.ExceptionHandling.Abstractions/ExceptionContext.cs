using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Represents the execution context in which the exception is handled by registered handlers.
    /// </summary>
    public class ExceptionContext
    {
        /// <summary>
        /// The originally thrown exception.
        /// </summary>
        public Exception OriginalException { get; }

        /// <summary>
        /// The new exception used to wrap or replace the <seealso cref="OriginalException"/>.
        /// </summary>
        /// <remarks>It is the <seealso cref="OriginalException"/> if not exolicitly specified.</remarks>
        public Exception Exception { get; set; }

        /// <summary>
        /// A <see cref="Guid"/> to uniquely identify the current exception handler operation.
        /// </summary>
        public Guid HandlingId { get; }

        /// <summary>
        /// A dictionary to store any properties which may be used by a particular exception handler.
        /// </summary>
        /// <remarks>The key of the dictionary is case sensitive.</remarks>
        public IDictionary<string, object> Properties { get; } 

        /// <summary>
        /// Create a new <see cref="ExceptionContext"/>.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="exception"/> is null.</exception>
        public ExceptionContext(Exception exception)
        {
            this.Exception = this.OriginalException = Guard.ArgumentNotNull(exception, nameof(exception));
            this.HandlingId = Guid.NewGuid();
            this.Properties = new Dictionary<string, object>();
        }
    }
}
