using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    public class VirtualMethodDynamicProxyGenerator : ITypeDynamicProxyGenerator
    {
        public bool CanIntercept(Type type)
        {
            return !Guard.ArgumentNotNull(type, nameof(type)).IsSealed;
        }

        public object Create(Type type, InterceptorDecoration interceptors, IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNull(interceptors, nameof(interceptors));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            if (this.CanIntercept(type))
            {
                var factory = DynamicProxyFactoryCache.Instance.GetTypeFactory(type, interceptors);
                return factory(interceptors, serviceProvider);
            }
            return serviceProvider.GetService(type);
        }
    }
} 