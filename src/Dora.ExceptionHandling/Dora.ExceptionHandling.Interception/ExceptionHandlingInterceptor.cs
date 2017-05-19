using Dora.Interception;
using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling.Interception
{
    /// <summary>
    /// An interceptor to use register <see cref="ExceptionManager"/> to handle the exception thrown from the target method.
    /// </summary>
    public class ExceptionHandlingInterceptor
    {
        private readonly InterceptDelegate _next;
        private readonly ExceptionManager _exceptionManager;
        private readonly string _exceptionPolicyName;

        /// <summary>
        /// Create a new <see cref="ExceptionHandlingInterceptor"/>.
        /// </summary>
        /// <param name="next">An <see cref="InterceptDelegate"/> used to invoke the next interceptor or target method.</param>
        /// <param name="exceptionManager">An <see cref="ExceptionManager"/> to handle the exception thrown from the target method.</param>
        /// <param name="exceptionPolicyName">The name of exception policy. The default exception policy will be used if not explicitly specified.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="next"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionManager"/> is null.</exception>
        public ExceptionHandlingInterceptor(InterceptDelegate next, ExceptionManager exceptionManager, string exceptionPolicyName = null)
        {
            _next = Guard.ArgumentNotNull(next, nameof(next));
            _exceptionManager = Guard.ArgumentNotNull(exceptionManager, nameof(exceptionManager));
            _exceptionPolicyName = exceptionPolicyName;
        }

        /// <summary>
        /// Invoke the next interceptor or target method and handle the thrown exception.
        /// </summary>
        /// <param name="invocationcontext">An <see cref="InvocationContext"/> representing the method invocation against the proxy.</param>
        /// <returns>The task to invoke the next interceptor or target method and handle the thrown exception.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="invocationcontext"/> is null.</exception>
        /// <remarks>Once an exception is thrown, the current <see cref="InvocationContext"/> represented by <paramref name="invocationcontext"/> will be added in the <see cref="ExceptionContext"/>, so the exception handler can get the current invocation based contextual information.</remarks>
        public async Task InvokeAsync(InvocationContext invocationcontext)
        {
            try
            {
                await _next(Guard.ArgumentNotNull(invocationcontext, nameof(invocationcontext)));
            }
            catch (Exception ex)
            {
                Action<ExceptionContext> initializer = _ => _.SetInvocationContext(invocationcontext);
                if (string.IsNullOrWhiteSpace(_exceptionPolicyName))
                {
                    await _exceptionManager.HandleExceptionAsync(ex, initializer);
                }
                else
                {
                    await _exceptionManager.HandleExceptionAsync(ex, _exceptionPolicyName, initializer);
                }
            }
        }
    }
}
