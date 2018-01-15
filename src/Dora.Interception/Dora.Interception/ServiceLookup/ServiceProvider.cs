// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using Dora.Interception;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    public sealed class ServiceProvider2 : IServiceProvider, IDisposable
    {
        // CallSiteRuntimeResolver is stateless so can be shared between all instances
        private static readonly CallSiteRuntimeResolver _callSiteRuntimeResolver = new CallSiteRuntimeResolver();
        private static readonly Func<Type, ServiceProvider2, Func<ServiceProvider2, object>> _createServiceAccessor = CreateServiceAccessor;

        private readonly CallSiteValidator _callSiteValidator;
        private bool _disposeCalled;
        private List<IDisposable> _disposables;

        // For testing only
        internal Action<object> _captureDisposableCallback;

        internal ServiceProvider2 Root { get; }
        internal CallSiteFactory CallSiteFactory { get; }
        internal Dictionary<object, object> ResolvedServices { get; }
        internal ConcurrentDictionary<Type, Func<ServiceProvider2, object>> RealizedServices { get; } = new ConcurrentDictionary<Type, Func<ServiceProvider2, object>>();

        //Added by Jiang Jin Nan:
        internal IInterceptingProxyFactory  ProxyFactory { get; }
        internal ServiceProvider2(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options, IInterceptingProxyFactory proxyFactory)
        {
            Root = this;

            if (options.ValidateScopes)
            {
                _callSiteValidator = new CallSiteValidator();
            }

            CallSiteFactory = new CallSiteFactory(serviceDescriptors, proxyFactory);
            ResolvedServices = new Dictionary<object, object>();

            CallSiteFactory.Add(typeof(IServiceProvider), new ServiceProviderCallSite());
            CallSiteFactory.Add(typeof(IServiceScopeFactory), new ServiceScopeFactoryCallSite());

            ProxyFactory = proxyFactory;
        }

        // This constructor is called exclusively to create a child scope from the parent
        internal ServiceProvider2(ServiceProvider2 parent)
        {
            Root = parent.Root;
            ResolvedServices = new Dictionary<object, object>();
            CallSiteFactory = parent.CallSiteFactory;
            RealizedServices = parent.RealizedServices;
            _callSiteValidator = parent._callSiteValidator;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            var realizedService = RealizedServices.GetOrAdd(serviceType, _createServiceAccessor, this);

            _callSiteValidator?.ValidateResolution(serviceType, this);

            return realizedService.Invoke(this);
        }

        private static Func<ServiceProvider2, object> CreateServiceAccessor(Type serviceType, ServiceProvider2 serviceProvider)
        {
            var callSite = serviceProvider.CallSiteFactory.CreateCallSite(serviceType, new HashSet<Type>());
            if (callSite != null)
            {
                serviceProvider._callSiteValidator?.ValidateCallSite(serviceType, callSite);
                return RealizeService(serviceType, callSite);
            }

            return _ => null;
        }

        internal static Func<ServiceProvider2, object> RealizeService(Type serviceType, IServiceCallSite callSite)
        {
            var callCount = 0;
            return provider =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    Task.Run(() =>
                    {
                        var realizedService = new CallSiteExpressionBuilder(_callSiteRuntimeResolver)
                            .Build(callSite);
                        provider.RealizedServices[serviceType] = realizedService;
                    });
                }

                return _callSiteRuntimeResolver.Resolve(callSite, provider);
            };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (ResolvedServices)
            {
                if (_disposeCalled)
                {
                    return;
                }

                _disposeCalled = true;
                if (_disposables != null)
                {
                    for (int i = _disposables.Count - 1; i >= 0; i--)
                    {
                        var disposable = _disposables[i];
                        disposable.Dispose();
                    }

                    _disposables.Clear();
                }

                ResolvedServices.Clear();
            }
        }

        internal object CaptureDisposable(object service)
        {
            _captureDisposableCallback?.Invoke(service);

            if (!object.ReferenceEquals(this, service))
            {
                var disposable = service as IDisposable;
                if (disposable != null)
                {
                    lock (ResolvedServices)
                    {
                        if (_disposables == null)
                        {
                            _disposables = new List<IDisposable>();
                        }

                        _disposables.Add(disposable);
                    }
                }
            }
            return service;
        }
    }
}
