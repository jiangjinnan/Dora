using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Represents a builder to build an exception handler chain.
    /// </summary>
    public interface IExceptionHandlerBuilder
    {
        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Regiser an exception handler represented by a <see cref="Func{ExceptionContext, Task}"/>.
        /// </summary>
        /// <param name="handler">A <see cref="Func{ExceptionContext, Task}"/> representing an exception handler to register.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/> with registered exception handler.</returns>
        IExceptionHandlerBuilder Use(Func<ExceptionContext, Task> handler);

        /// <summary>
        /// Build an exception handler chain.
        /// </summary>
        /// <returns>A <see cref="Func{ExceptionContext, Task}"/> representing the exception handler chain.</returns>
        Func<ExceptionContext, Task> Build();
    }
}
