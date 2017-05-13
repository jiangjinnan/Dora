using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Default some extension methods specific to <see cref="IExceptionPolicyBuilder"/>.
    /// </summary>
    public static class ExceptionPolicyBuilderExtensions
    {
        /// <summary>
        /// Register exception handler chain for specified exception type.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionPolicyBuilder"/> to which the type specific hanlder chain is registered.</param>
        /// <typeparam name="TException">The type of exception to handle.</typeparam>
        /// <param name="postHandlingAction">Determining what action should occur after an exception is handled by the configured exception handling chain.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        public static IExceptionPolicyBuilder For<TException>(this IExceptionPolicyBuilder builder, PostHandlingAction postHandlingAction, Action<IExceptionHandlerBuilder> configure)
            where TException : Exception
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UseHandlers(typeof(TException), postHandlingAction, configure);
        }

        /// <summary>
        /// Register common exception handler chain which is invoked before the ones registered to exception type.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionPolicyBuilder"/> to which the pre-hanlder-chain is registered.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        public static IExceptionPolicyBuilder UsePreHandlers(this IExceptionPolicyBuilder builder, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UsePreHandlers(_ => true, configure);
        }

        /// <summary>
        /// Register a common exception handler chain which is invoked after the ones registered to exception type.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionPolicyBuilder"/> to which the pre-hanlder-chain is registered.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        public static IExceptionPolicyBuilder UsePostHandlers(this IExceptionPolicyBuilder builder, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UsePostHandlers(_ => true, configure);
        }

        /// <summary>
        /// Register a common exception handler chain.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionPolicyBuilder"/> to which the common exception handler chain is registered.</param>
        /// <param name="predicate">A filter used to determine whether the registered exception handler should be invoked.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        public static IExceptionPolicyBuilder UseHandlers(this IExceptionPolicyBuilder builder, Func<Exception, bool> predicate, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UsePostHandlers(predicate, configure);
        }

        /// <summary>
        /// Register a common exception handler chain.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionPolicyBuilder"/> to which the common exception handler chain is registered.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        public static IExceptionPolicyBuilder UseHandlers(this IExceptionPolicyBuilder builder,  Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UsePostHandlers(configure);
        }
    }
}
