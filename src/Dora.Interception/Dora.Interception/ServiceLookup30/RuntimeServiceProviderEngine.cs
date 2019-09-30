// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Dora.Interception;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class RuntimeServiceProviderEngine : ServiceProviderEngine
    {
        public RuntimeServiceProviderEngine(IEnumerable<ServiceDescriptor> serviceDescriptors, IServiceProviderEngineCallback callback, IInterceptingProxyFactory proxyFactory ) 
            : base(serviceDescriptors, callback, proxyFactory)
        {
        }

        protected override Func<ServiceProviderEngineScope, object> RealizeService(ServiceCallSite callSite)
        {
            return scope =>
            {
                object RealizedService(ServiceProviderEngineScope p) => RuntimeResolver.Resolve(callSite, p);
                RealizedServices[callSite.ServiceType] = RealizedService;
                return RealizedService(scope);
            };
        }
    }
}