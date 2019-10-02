using System;

namespace Dora.Interception
{
    /// <summary>
    /// A global cache for dynamic proxy factories.
    /// </summary>
    public interface IInterceptableProxyFactoryCache
    {
        /// <summary>
        /// Get instance based dynamic proxy factory.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <param name="interceptors">The interceptor decoration applied to <paramref name="type"/>.</param>
        /// <returns>The instance based dynamic proxy factory.</returns>
        Func<object, object> GetInstanceFactory(Type type, IInterceptorRegistry interceptors);

        /// <summary>
        /// Get type based dynamic proxy factory.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <param name="interceptors">The interceptor decoration applied to <paramref name="type"/>.</param>
        /// <returns>The type based dynamic proxy factory.</returns>
        Func<IServiceProvider, object> GetTypeFactory(Type type, IInterceptorRegistry interceptors);
    }
}
