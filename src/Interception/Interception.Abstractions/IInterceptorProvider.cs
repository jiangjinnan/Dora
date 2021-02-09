using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get <see cref="IInterceptor"/> assigned to specified method.
    /// </summary>
    public interface IInterceptorProvider
    {
        /// <summary>
        /// Gets the interceptor assigned to specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The interceptor.</returns>
        IInterceptor GetInterceptor(MethodInfo method);
    }
}
