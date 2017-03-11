using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a builder to build an interceptor chain.
    /// </summary>
    public interface IInterceptorChainBuilder
    {
        /// <summary>
        /// Gets the service provider to get dependency services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Register specified interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor to register.</param>
        /// <param name="order">The order for the registered interceptor in the interceptor chain.</param>
        /// <returns>The interceptor chain builder with registered intercetor.</returns>
        IInterceptorChainBuilder Use(InterceptorDelegate interceptor, int order);

        /// <summary>
        /// Build an interceptor chain using the registerd interceptors.
        /// </summary>
        /// <returns>A composite interceptor representing the interceptor chain.</returns>
        InterceptorDelegate Build();

        /// <summary>
        /// Create a new interceptor chain builder which owns the same service provider as the current one.
        /// </summary>
        /// <returns>The new interceptor to create.</returns>
        IInterceptorChainBuilder New();
    }
}
