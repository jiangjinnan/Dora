using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Represents an exception handling policy.
    /// </summary>
    public interface IExceptionPolicy
    {
        /// <summary>
        /// Create an exception handler based on specified exception to handle.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="postHandlingAction">A <see cref="PostHandlingAction"/> determining what action should occur after an exception is handled by the configured exception handling chain. </param>
        /// <returns>A <see cref="Func{TExceptionContext, Task}"/> representing the exception handler.</returns>
        Func<ExceptionContext, Task> CreateHandler(Exception exception, out PostHandlingAction postHandlingAction);
    }
}
