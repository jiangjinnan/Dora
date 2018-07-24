using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Define some extension methods specific to <see cref="IInterceptingProxyFactory"/>.
    /// </summary>
    public static class InterceptingProxyFactoryExtensions
    {
        /// <summary>
        /// Create the specified type of proxy to wrap the specified target instance.
        /// </summary>
        /// <typeparam name="T">The type of proxy.</typeparam>
        /// <param name="proxyFactory">The proxy factory.</param>
        /// <param name="target">The target object wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target.</returns>   
        /// <exception cref="ArgumentNullException">Specified <paramref name="proxyFactory"/> is null.</exception>    
        /// <exception cref="ArgumentNullException">Specified <paramref name="target"/> is null.</exception>  
        public static T Wrap<T>(this IInterceptingProxyFactory proxyFactory, T target) where T: class
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            Guard.ArgumentNotNull(target, nameof(target));
            return (T)proxyFactory.Wrap(typeof(T), target);
        }

        /// <summary>
        /// Create the specified type of proxy.
        /// </summary>
        /// <typeparam name="T">The type of proxy.</typeparam>
        /// <param name="proxyFactory">The proxy factory.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The interceptable proxy.</returns>
        /// <exception cref="ArgumentException">Invalid ServiceProvider - serviceProvider</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="proxyFactory" /> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="proxyFactory" /> is null.</exception>
        public static T Create<T>(this IInterceptingProxyFactory proxyFactory, IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            if (serviceProvider is InterceptableServiceProvider)
            {
                throw new ArgumentException("Invalid ServiceProvider", nameof(serviceProvider));
            }

            return (T)proxyFactory.Create(typeof(T), serviceProvider);
        }
    }
}
