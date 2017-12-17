using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception.ServiceLookup
{
    internal class InterceptionCallSite : IServiceCallSite
    {
        public IProxyFactory ProxyFactory { get; }
        public IServiceCallSite TargetCallSite { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        public InterceptionCallSite(IProxyFactory proxyFactory, IServiceCallSite targetCallSite)
        {
            this.ProxyFactory = proxyFactory;
            this.TargetCallSite = targetCallSite;
            this.ServiceType = targetCallSite.ServiceType;
            this.ImplementationType = targetCallSite.ImplementationType;
        }
    }
}
