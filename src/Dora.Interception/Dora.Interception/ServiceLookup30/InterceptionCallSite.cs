using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class InterceptionCallSite : ServiceCallSite
    {
        public override Type ServiceType { get; }
        public override Type ImplementationType { get; }
        public override CallSiteKind Kind => CallSiteKind.Interception;
        public bool CanIntercept { get;  }
        public IInterceptingProxyFactory ProxyFactory { get; }
        public ServiceCallSite Target { get; }

        public InterceptionCallSite(Type serviceType, Type implementationType, IInterceptingProxyFactory proxyFactory, ServiceCallSite target)
            :base(ResultCache.None)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            CanIntercept = serviceType.IsInterface || target is ConstructorCallSite;
            ProxyFactory = proxyFactory ?? throw new ArgumentNullException(nameof(proxyFactory));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

    }
}
