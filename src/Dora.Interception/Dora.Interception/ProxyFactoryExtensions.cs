using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Define some extension methods specific to <see cref="IInterceptingProxyFactory"/>.
    /// </summary>
    public static class ProxyFactoryExtensions
    {
        /// <summary>
        /// Create the specified type of proxy.
        /// </summary>
        /// <typeparam name="T">The type of proxy.</typeparam>
        /// <param name="proxyFactory">The proxy factory.</param>
        /// <param name="target">The target object wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target.</returns>
        public static T CreateProxy<T>(this IInterceptingProxyFactory proxyFactory, T target) 
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            return (T)proxyFactory.Wrap(typeof(T), target);
        }
    }
}
