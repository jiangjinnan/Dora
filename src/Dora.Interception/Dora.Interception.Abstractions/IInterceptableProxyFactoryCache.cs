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
        /// <param name="interface">The type to intercept.</param>
        /// <param name="targetType"></param>
        /// <returns>The instance based dynamic proxy factory.</returns>
        Func<object, object> GetInstanceFactory(Type @interface, Type targetType);

        /// <summary>
        /// Get type based dynamic proxy factory.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <returns>The type based dynamic proxy factory.</returns>
        Func<IServiceProvider, object> GetTypeFactory(Type type);
    }
}
