using Dora.Interception;
using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling.Interception
{
    public class ExceptionHandlingInterceptor
    {
        private readonly InterceptDelegate _next;
        private readonly ExceptionManager _exceptionManager;
        private readonly string _exceptionPolicyName;

        public ExceptionHandlingInterceptor(InterceptDelegate next, ExceptionManager exceptionManager, string exceptionPolicyName = null)
        {
            _next = Guard.ArgumentNotNull(next, nameof(next));
            _exceptionManager = Guard.ArgumentNotNull(exceptionManager, nameof(exceptionManager));
            _exceptionPolicyName = exceptionPolicyName;
        }

        public async Task InvokeAsync(InvocationContext invocationcontext)
        {
            try
            {
                await _next(invocationcontext);
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
