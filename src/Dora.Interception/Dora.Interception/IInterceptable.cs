using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{ 
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
        private readonly IInterceptingProxyFactory _proxyFactory;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Create a new <see cref="Interceptable{T}"/>
        /// </summary>
        /// <param name="proxyFactory">The service factory to create the proxy to wrapping the target service instance.</param>
        /// <param name="serviceProvider">The service provider to provide target service instances.</param>
        public Interceptable(IInterceptingProxyFactory proxyFactory, IServiceProvider serviceProvider)
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
            get
            {
                if (null != _proxy)
                {
                    return _proxy;
                }

                if (typeof(T).IsInterface)
                {
                    var target = _serviceProvider.GetService<T>();
                    return _proxy = _proxyFactory.Wrap<T>(target);
                }

                return _proxy = _proxyFactory.Create<T>(_serviceProvider); 
            }  
        }
    }
}
