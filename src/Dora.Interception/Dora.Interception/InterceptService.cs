using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    internal class InterceptService: Service
    {
        public IProxyFactory ProxyFactory { get; }
        public InterceptService(ServiceDescriptor descriptor, IProxyFactory proxyFactory) : base(descriptor)
        {
            this.ProxyFactory = proxyFactory;
        }

        public override IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain)
        {
            var targetCallSite = base.CreateCallSite(provider, callSiteChain);
            return new IntercepCallSite(this.ProxyFactory,targetCallSite, this.ServiceType);
        }
    }
}
