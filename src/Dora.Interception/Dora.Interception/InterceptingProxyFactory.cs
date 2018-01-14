using Dora.DynamicProxy;
using System;

namespace Dora.Interception
{
    public class InterceptingProxyFactory : IInterceptingProxyFactory
    {
        public IInstanceDynamicProxyGenerator DynamicProxyGenerator { get; }
        public IInterceptorCollector InterceptorCollector { get; }
        public IServiceProvider ServiceProvider { get; }
        public InterceptingProxyFactory(
            IInstanceDynamicProxyGenerator dynamicProxyGenerator,
            IInterceptorCollector interceptorCollector,
            IServiceProvider serviceProvider)
        {
            this.DynamicProxyGenerator = Guard.ArgumentNotNull(dynamicProxyGenerator, nameof(dynamicProxyGenerator));
            this.InterceptorCollector = Guard.ArgumentNotNull(interceptorCollector, nameof(interceptorCollector));
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        public object CreateProxy(Type typeToProxy, object target)
        {
            Guard.ArgumentNotNull(typeToProxy, nameof(typeToProxy));
            Guard.ArgumentNotNull(target,nameof(target));
            Guard.ArgumentAssignableTo(typeToProxy, target.GetType(), nameof(target));

            var interceptors = this.InterceptorCollector.GetInterceptors(typeToProxy, target.GetType());
            if (interceptors.MethodBasedInterceptors.Count == 0 && interceptors.PropertyBasedInterceptors.Count == 0)
            {
                return target;
            }

            return this.DynamicProxyGenerator.Wrap(typeToProxy, target, interceptors);
        }
    }
}
