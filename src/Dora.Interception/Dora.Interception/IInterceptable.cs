using Dora.Interception.Internal;
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
    internal class Interceptable<T> : IInterceptable<T> where T : class
    {
        private readonly bool _isInterceptableServiceProvider;
        private readonly IInterceptingProxyFactory _proxyFactory;
        private readonly IServiceProvider _serviceProvider;
        
        public Interceptable(IInterceptingProxyFactory proxyFactory, IServiceProvider serviceProvider, IInterceptableServiceProviderIndicator indicator)
        {
            Guard.ArgumentNotNull(proxyFactory, nameof(proxyFactory));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            _proxyFactory = proxyFactory;
            _serviceProvider = serviceProvider;
            _isInterceptableServiceProvider = indicator.IsInterceptable;
        }
        
        public T Proxy
        {
            get
            {
                if (typeof(T).IsInterface && !_isInterceptableServiceProvider)
                {
                    var target = _serviceProvider.GetService<T>();
                    return (T)_proxyFactory.Wrap(typeof(T), target);
                }
                return _serviceProvider.GetService<T>();
            }  
        }
    }
}
