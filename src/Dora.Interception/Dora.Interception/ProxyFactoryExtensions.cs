using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public static class ProxyFactoryExtensions
    {
        public static T CreateProxy<T>(this IProxyFactory proxyFactory, T target) 
        {
            return (T)proxyFactory.CreateProxy(typeof(T), target);
        }
    }
}
