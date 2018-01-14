using System;

namespace Dora.DynamicProxy
{
    public class InterfaceDynamicProxyGenerator : IInstanceDynamicProxyGenerator
    {
        public bool CanIntercept(Type type)
        {
            return Guard.ArgumentNotNull(type, nameof(type)).IsInterface;
        }

        public object Wrap(Type type, object target, InterceptorDecoration interceptors)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNull(target, nameof(target));
            Guard.ArgumentNotNull(interceptors, nameof(interceptors));

            if (this.CanIntercept(type))
            {
                var factory = DynamicProxyFactoryCache.Instance.GetInstanceFactory(type, interceptors);
                return factory(target, interceptors);
            }

            return target;
        }
    }
}
