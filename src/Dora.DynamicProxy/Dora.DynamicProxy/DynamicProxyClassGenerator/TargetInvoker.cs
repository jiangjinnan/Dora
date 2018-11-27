using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Define a method to invoke interceptor and target method.
    /// </summary>
    public static class TargetInvoker
    {
        /// <summary>
        ///  Invoke interceptor and target method.
        /// </summary>
        /// <param name="interceptor">A <see cref="InterceptorDelegate"/> representing interceptor applied to target method.</param>
        /// <param name="handler">A <see cref="InterceptDelegate"/> used to invoke the target method.</param>
        /// <param name="context">A <see cref="InvocationContext"/> representing the current method invocation context.</param>
        /// <returns>The task to invoke interceptor and target method.</returns>
        public static Task InvokeHandler(InterceptorDelegate interceptor, InterceptDelegate handler, InvocationContext context)
        {
            InterceptDelegate wrapper = async _ => {
                await handler(_);
                var task = _.ReturnValue as Task;
                if (null != task)
                {
                    await task;
                }
            };
            return interceptor(wrapper)(context);
        }
    }
} 