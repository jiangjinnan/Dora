using System;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Represents a builder to build an exception policy.
    /// </summary>
    public interface IExceptionPolicyBuilder
    {
        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Register exception handler chain for specified exception type.
        /// </summary>
        /// <param name="exceptionType">The type of exception handled by the registered exception handler chain.</param>
        /// <param name="postHandlerAction">Determining what action should occur after an exception is handled by the configured exception handling chain.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        IExceptionPolicyBuilder For(Type exceptionType, PostHandlingAction postHandlerAction, Action<IExceptionHandlerBuilder> configure);

        /// <summary>
        /// Register common exception handler chain which is invoked before the ones registered to exception type.
        /// </summary>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        IExceptionPolicyBuilder Pre(Action<IExceptionHandlerBuilder> configure);

        /// <summary>
        /// Register common exception handler chain which is invoked after the ones registered to exception type.
        /// </summary>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        IExceptionPolicyBuilder Post(Action<IExceptionHandlerBuilder> configure);

        /// <summary>
        /// Build the exception policy.
        /// </summary>
        /// <returns>The exception policy to build.</returns>
        IExceptionPolicy Build();
    }
}
