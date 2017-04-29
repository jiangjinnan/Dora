using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    internal class IntercepCallSite: IServiceCallSite
    {
        public IProxyFactory ProxyFactory { get; }
        public IServiceCallSite TargetCallSite { get; }
        public Type ServiceType { get; }

        public IntercepCallSite(IProxyFactory proxyFactory, IServiceCallSite targetCallSite, Type serviceType)
        {
            this.ProxyFactory = proxyFactory;
            this.TargetCallSite = targetCallSite;
            this.ServiceType = serviceType;
        }
    }
}