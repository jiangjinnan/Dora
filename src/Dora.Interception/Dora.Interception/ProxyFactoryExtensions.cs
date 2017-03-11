using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Define some extension methods specific to <see cref="IProxyFactory"/>.
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
        public static T CreateProxy<T>(this IProxyFactory proxyFactory, T target) 
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            return (T)proxyFactory.CreateProxy(typeof(T), target);
        }
    }
}
