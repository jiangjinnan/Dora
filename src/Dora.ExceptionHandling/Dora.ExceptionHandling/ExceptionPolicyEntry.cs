using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Represents a policy entry specific to an exception type.
    /// </summary>
    public class ExceptionPolicyEntry
    {
        /// <summary>
        /// The type of exception to handle.
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// Determining what action should occur after an exception is handled by the configured exception handling chain.
        /// </summary>
        public PostHandlingAction PostHandlingAction { get; }

        /// <summary>
        /// A <see cref="Func{ExceptionContext, Task}"/> representing the exception handler chain.
        /// </summary>
        public Func<ExceptionContext, Task> Handler { get; }

        /// <summary>
        /// Create a new <see cref="ExceptionPolicyEntry"/>.
        /// </summary>
        /// <param name="exceptionType">The type of exception to handle.</param>
        /// <param name="postHandlingAction">Determining what action should occur after an exception is handled by the configured exception handling chain.</param>
        /// <param name="exceptionHandler">A <see cref="Func{ExceptionContext, Task}"/> representing the exception handler chain.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionType"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="postHandlingAction"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionHandler"/> is null.</exception>
        public ExceptionPolicyEntry(Type exceptionType, PostHandlingAction postHandlingAction, Func<ExceptionContext, Task> exceptionHandler)
        {
            this.ExceptionType = Guard.ArgumentAssignableTo<Exception>(exceptionType, nameof(exceptionType));
            this.Handler =Guard.ArgumentNotNull(exceptionHandler, nameof(exceptionHandler));
            this.PostHandlingAction = postHandlingAction;
        }
    }
}
