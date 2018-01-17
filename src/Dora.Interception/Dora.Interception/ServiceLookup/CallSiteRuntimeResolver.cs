using System;
using System.Runtime.ExceptionServices;
using Dora.Interception.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteRuntimeResolver : CallSiteVisitor<ServiceProvider2, object>
    {
        public object Resolve(IServiceCallSite callSite, ServiceProvider2 provider)
        {
            return VisitCallSite(callSite, provider);
        }

        protected override object VisitTransient(TransientCallSite transientCallSite, ServiceProvider2 provider)
        {
            return provider.CaptureDisposable(
                VisitCallSite(transientCallSite.ServiceCallSite, provider));
        }

        protected override object VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProvider2 provider)
        {
            object[] parameterValues = new object[constructorCallSite.ParameterCallSites.Length];
            for (var index = 0; index < parameterValues.Length; index++)
            {
                parameterValues[index] = VisitCallSite(constructorCallSite.ParameterCallSites[index], provider);
            }

            try
            {
                return constructorCallSite.ConstructorInfo.Invoke(parameterValues);
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // The above line will always throw, but the compiler requires we throw explicitly.
                throw;
            }
        }

        protected override object VisitSingleton(SingletonCallSite singletonCallSite, ServiceProvider2 provider)
        {
            return VisitScoped(singletonCallSite, provider.Root);
        }

        protected override object VisitScoped(ScopedCallSite scopedCallSite, ServiceProvider2 provider)
        {
            lock (provider.ResolvedServices)
            {
                if (!provider.ResolvedServices.TryGetValue(scopedCallSite.CacheKey, out var resolved))
                {
                    resolved = VisitCallSite(scopedCallSite.ServiceCallSite, provider);
                    provider.CaptureDisposable(resolved);
                    provider.ResolvedServices.Add(scopedCallSite.CacheKey, resolved);
                }
                return resolved;
            }
        }

        protected override object VisitConstant(ConstantCallSite constantCallSite, ServiceProvider2 provider)
        {
            return constantCallSite.DefaultValue;
        }

        protected override object VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, ServiceProvider2 provider)
        {
            try
            {
                return Activator.CreateInstance(createInstanceCallSite.ImplementationType);
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // The above line will always throw, but the compiler requires we throw explicitly.
                throw;
            }
        }

        protected override object VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, ServiceProvider2 provider)
        {
            return provider;
        }

        protected override object VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, ServiceProvider2 provider)
        {
            return new ServiceScopeFactory(provider);
        }

        protected override object VisitIEnumerable(IEnumerableCallSite enumerableCallSite, ServiceProvider2 provider)
        {
            var array = Array.CreateInstance(
                enumerableCallSite.ItemType,
                enumerableCallSite.ServiceCallSites.Length);

            for (var index = 0; index < enumerableCallSite.ServiceCallSites.Length; index++)
            {
                var value = VisitCallSite(enumerableCallSite.ServiceCallSites[index], provider);
                array.SetValue(value, index);
            }
            return array;
        }

        protected override object VisitFactory(FactoryCallSite factoryCallSite, ServiceProvider2 provider)
        {
            return factoryCallSite.Factory(provider);
        }

        protected override object VisitInterception(InterceptionCallSite interceptionCallSite, ServiceProvider2 provider)
        {
            if (interceptionCallSite.ServiceType.IsInterface)
            {
                var target = this.VisitCallSite(interceptionCallSite.TargetCallSite, provider);
                return interceptionCallSite.ProxyFactory.Wrap(interceptionCallSite.ServiceType, target);
            }

            if (interceptionCallSite.TargetCallSite is ConstructorCallSite || interceptionCallSite.TargetCallSite is CreateInstanceCallSite)
            {
                return interceptionCallSite.ProxyFactory.Create(interceptionCallSite.ServiceType, provider, () => VisitCallSite(interceptionCallSite.TargetCallSite, provider));
            }

            return this.VisitCallSite(interceptionCallSite.TargetCallSite, provider);
        }
    }
}