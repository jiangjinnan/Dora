using System;

namespace Dora.Interception
{
    /// <summary>
    /// Registry interceptors are registered to.
    /// </summary>
    public interface IInterceptorRegistry: IInterceptorRegistrationProvider
    {
        /// <summary>
        /// Registers specified type of interceptor.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        /// <param name="assignment">The interceptor assignment delegate.</param>
        /// <param name="arguments">The arguments for interceptor activation.</param>
        /// <returns>The current interceptor registry.</returns>
        IInterceptorRegistry Register<TInterceptor>(Action<IInterceptorAssigner> assignment, params object[] arguments);
    }
}
