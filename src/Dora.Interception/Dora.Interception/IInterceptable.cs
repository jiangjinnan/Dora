using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    public interface IInterceptable
    {
    }

    public interface IInterceptable<T> where T : class
    {
        T Proxy { get; }
    }

    public class Interceptable<T> : IInterceptable<T> where T:class
    {
        private T _proxy;
        private IProxyFactory _proxyFactory;
        private IServiceProvider _serviceProvider;
        public Interceptable(IProxyFactory proxyFactory, IServiceProvider serviceProvider)
        {
            _proxyFactory = proxyFactory;
            _serviceProvider = serviceProvider;
        }
        public T Proxy
        {
           get { return _proxy ?? (_proxy = _proxyFactory.CreateProxy<T>(_serviceProvider.GetService<T>())); }
        }
    }
}
