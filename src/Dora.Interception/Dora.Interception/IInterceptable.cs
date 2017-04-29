using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    /// <summary>
    /// A marker interface representing the service which can be intercepted.
    /// </summary>
    public interface IInterceptable
    {
    }

    /// <summary>
    /// Represents the service with a proxy against which the method invocation can be intercepted.
    /// </summary>
    /// <typeparam name="T">The declaration type of service.</typeparam>
    public interface IInterceptable<T> where T : class
    {
        /// <summary>
        /// The proxy against which the method invocation can be intercepted.
        /// </summary>
        T Proxy { get; }
    }

    /// <summary>
    /// Represents the service with a proxy against which the method invocation can be intercepted.
    /// </summary>
    /// <typeparam name="T">The declaration type of service.</typeparam>
    public class Interceptable<T> : IInterceptable<T> where T : class
    {
        private T _proxy;
        private readonly IProxyFactory _proxyFactory;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Create a new <see cref="Interceptable{T}"/>
        /// </summary>
        /// <param name="proxyFactory">The service factory to create the proxy to wrapping the target service instance.</param>
        /// <param name="serviceProvider">The service provider to provide target service instances.</param>
        public Interceptable(IProxyFactory proxyFactory, IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            _proxyFactory = proxyFactory;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// The proxy against which the method invocation can be intercepted.
        /// </summary>
        public T Proxy
        {
            get { return _proxy ?? (_proxy = _proxyFactory.CreateProxy<T>(_serviceProvider.GetService<T>())); }
        }
    }
}
