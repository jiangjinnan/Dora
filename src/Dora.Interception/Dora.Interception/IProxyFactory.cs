using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a factory to create a proxy wrapping specified target instance.
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// Get a service provider to get dependency services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a proxy wrapping specified target instance. 
        /// </summary>
        /// <param name="typeToProxy">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        object CreateProxy(Type typeToProxy, object target);
    }
}
