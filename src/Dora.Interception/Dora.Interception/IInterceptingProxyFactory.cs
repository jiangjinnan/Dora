using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a factory to create a proxy wrapping specified target instance.
    /// </summary>
    public interface IInterceptingProxyFactory
    {
        /// <summary>
        /// Get a service provider to get dependency services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a proxy wrapping specified target instance. 
        /// </summary>
        /// <param name="typeToIntercept">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        object Wrap(Type typeToIntercept, object target);

        /// <summary>
        /// Creates the specified type to proxy.
        /// </summary>
        /// <param name="typeToIntercept">The type to proxy.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="targetAccessor">The target instance accessor.</param>
        /// <returns>
        /// The proxy wrapping the specified target instance.
        /// </returns>
        object Create(Type typeToIntercept, IServiceProvider serviceProvider, Func<object> targetAccessor = null);
    }
}
