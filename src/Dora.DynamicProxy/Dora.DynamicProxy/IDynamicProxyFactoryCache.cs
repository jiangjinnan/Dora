using System;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// A global cache for dynamic proxy factories.
    /// </summary>
    public interface IDynamicProxyFactoryCache
    {
        /// <summary>
        /// Get instance based dynamic proxy factory.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <param name="interceptors">The interceptor decoration applied to <paramref name="type"/>.</param>
        /// <returns>The instance based dynamic proxy factory.</returns>
        Func<object, InterceptorRegistry, object> GetInstanceFactory(Type type, InterceptorRegistry interceptors);

        /// <summary>
        /// Get type based dynamic proxy factory.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <param name="interceptors">The interceptor decoration applied to <paramref name="type"/>.</param>
        /// <returns>The type based dynamic proxy factory.</returns>
        Func<InterceptorRegistry, IServiceProvider, object> GetTypeFactory(Type type, InterceptorRegistry interceptors);
    }
}
