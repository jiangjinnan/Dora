// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    internal sealed class ServiceProvider2 : IServiceProvider, IDisposable, IServiceProviderEngineCallback
    {
        private readonly IServiceProviderEngine _engine;
        private readonly CallSiteValidator _callSiteValidator;
        private readonly IInterceptingProxyFactory _interceptingProxyFactory;

        internal ServiceProvider2(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options, IInterceptingProxyFactory interceptingProxyFactory)
        {
            IServiceProviderEngineCallback callback = null;
            _interceptingProxyFactory = interceptingProxyFactory;
            if (options.ValidateScopes)
            {
                callback = this;
                _callSiteValidator = new CallSiteValidator();
            }
            _engine = new ExpressionsServiceProviderEngine(serviceDescriptors, callback, interceptingProxyFactory);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType) => _engine.GetService(serviceType);

        /// <inheritdoc />
        public void Dispose() => _engine.Dispose();

        void IServiceProviderEngineCallback.OnCreate(IServiceCallSite callSite)
        {
            _callSiteValidator.ValidateCallSite(callSite);
        }

        void IServiceProviderEngineCallback.OnResolve(Type serviceType, IServiceScope scope)
        {
            _callSiteValidator.ValidateResolution(serviceType, scope, _engine.RootScope);
        }
    }
}
