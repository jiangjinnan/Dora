using Dora.Interception;
using System;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Define some extension methods specific to <see cref="ExceptionContext"/>.
    /// </summary>
    public static class ExceptionContextExtensions
    {
        private const string KeyOfInvocationContext = "Dora.Interception.InvocationContext";

        /// <summary>
        /// Add the <see cref="InvocationContext"/> to the specified specified <see cref="ExceptionContext"/>'s property dictionary.
        /// </summary>
        /// <param name="exceptionContext">The <see cref="ExceptionContext"/> in whose property dictionary the specified <see cref="InvocationContext"/> is added.</param>
        /// <param name="invocationContext">The <see cref="InvocationContext"/> added in the specified <see cref="ExceptionContext"/>'s property dictionary.</param>
        /// <returns>The <see cref="ExceptionContext"/> with <see cref="InvocationContext"/> based property.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionContext"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="invocationContext"/> is null.</exception>
        public static ExceptionContext SetInvocationContext(this ExceptionContext exceptionContext, InvocationContext invocationContext)
        {
            Guard.ArgumentNotNull(exceptionContext, nameof(exceptionContext));
            Guard.ArgumentNotNull(invocationContext, nameof(invocationContext));
            exceptionContext.Properties[KeyOfInvocationContext] = invocationContext;
            return exceptionContext;
        }

        /// <summary>
        /// Try to get the <see cref="InvocationContext"/> from the specified <see cref="ExceptionContext"/>'s property dictionary.
        /// </summary>
        /// <param name="exceptionContext">The <see cref="ExceptionContext"/> whose property dictionary containing the <see cref="InvocationContext"/>.</param>
        /// <param name="invocationContext">The <see cref="InvocationContext"/> loaded from the specified <see cref="ExceptionContext"/>'s property dictionary.</param>
        /// <returns>A <see cref="bool"/> value indicating whether to successfully get the <see cref="InvocationContext"/>.</returns>
        public static bool  TryGetInvocationContext(this ExceptionContext exceptionContext, out InvocationContext invocationContext)
        {
            Guard.ArgumentNotNull(exceptionContext, nameof(exceptionContext));
            invocationContext = null;
            if (exceptionContext.Properties.TryGetValue(KeyOfInvocationContext, out object context))
            {
                return false;
            }
            return (invocationContext = (context as InvocationContext)) != null;
        }
    }
}