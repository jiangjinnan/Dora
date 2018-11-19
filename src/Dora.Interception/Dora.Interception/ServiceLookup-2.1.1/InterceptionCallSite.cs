using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using System;

namespace Dora.Interception.ServiceLookup
{
    internal class InterceptionCallSite : IServiceCallSite
    {
        public IInterceptingProxyFactory ProxyFactory { get; }
        public IServiceCallSite TargetCallSite { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; } 
        public CallSiteKind Kind => CallSiteKind.Interception;     
        public bool CanIntercept { get; }
        public InterceptionCallSite(IInterceptingProxyFactory proxyFactory, IServiceCallSite targetCallSite)
        {
            ProxyFactory = proxyFactory;
            TargetCallSite = targetCallSite;
            ServiceType = targetCallSite.ServiceType;
            ImplementationType = targetCallSite.ImplementationType;
            CanIntercept = ServiceType.IsInterface || (targetCallSite.Kind != CallSiteKind.Factory && targetCallSite.Kind != CallSiteKind.Constant);
        }
    }
}
