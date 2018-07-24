using Dora.Interception;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ExpressionsServiceProviderEngine : ServiceProviderEngine
    {
        private readonly ExpressionResolverBuilder _expressionResolverBuilder;
        public ExpressionsServiceProviderEngine(
            IEnumerable<ServiceDescriptor> serviceDescriptors, 
            IServiceProviderEngineCallback callback,
            IInterceptingProxyFactory interceptingProxyFactory) : base(serviceDescriptors, callback, interceptingProxyFactory)
        {
            _expressionResolverBuilder = new ExpressionResolverBuilder(RuntimeResolver, this, Root);
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(IServiceCallSite callSite)
        {
            var realizedService = _expressionResolverBuilder.Build(callSite);
            RealizedServices[callSite.ServiceType] = realizedService;
            return realizedService;
        }
    }
}