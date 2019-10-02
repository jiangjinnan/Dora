using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    public class InterceptableServiceDescriptor
    {
        private readonly ServiceDescriptor _primaryDescriptor;
        private readonly ServiceDescriptor _secondaryDescriptor = null;
        public InterceptableServiceDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime,
            IInterceptorResolver interceptorResolver,
            IInterceptableProxyFactoryCache factoryCache,
            ICodeGeneratorFactory codeGeneratorFactory)
            : this(new ServiceDescriptor(serviceType, implementationType, lifetime), interceptorResolver, factoryCache, codeGeneratorFactory)
        { }

        public InterceptableServiceDescriptor(
            ServiceDescriptor serviceDescriptor,
            IInterceptorResolver interceptorResolver,
            IInterceptableProxyFactoryCache factoryCache,
            ICodeGeneratorFactory codeGeneratorFactory)
        {
            Guard.ArgumentNotNull(serviceDescriptor, nameof(serviceDescriptor));
            Guard.ArgumentNotNull(interceptorResolver, nameof(interceptorResolver));
            Guard.ArgumentNotNull(factoryCache, nameof(factoryCache));

            if (serviceDescriptor.ImplementationInstance != null || serviceDescriptor.ImplementationFactory != null)
            {
                _primaryDescriptor = serviceDescriptor;
                return;
            }
            var serviceType = serviceDescriptor.ServiceType;
            var implementationType = serviceDescriptor.ImplementationType;
            var lifetime = serviceDescriptor.Lifetime;

            if (serviceType.IsInterface)
            {
                var interceptors = interceptorResolver.GetInterceptors(serviceType, implementationType);
                if (interceptors.IsEmpty)
                {
                    _primaryDescriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                }
                else if (serviceType.IsGenericTypeDefinition)
                {
                    _secondaryDescriptor = new ServiceDescriptor(implementationType, implementationType);
                    var codeGenerator = codeGeneratorFactory.Create();
                    var context = new CodeGenerationContext(serviceType, implementationType, interceptors);
                    var proxyType = codeGenerator.GenerateInterceptableProxyClass(context);
                    _primaryDescriptor = new ServiceDescriptor(serviceType, proxyType);

                    _primaryDescriptor = serviceDescriptor;
                }
                else
                {
                    _primaryDescriptor = new ServiceDescriptor(serviceType, CreateOrGet, lifetime);
                    object CreateOrGet(IServiceProvider serviceProvider)
                    {
                        var target = ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
                        return factoryCache.GetInstanceFactory(serviceType, interceptors).Invoke(target);
                    }
                }
            }
            else
            {
                var interceptors = interceptorResolver.GetInterceptors(implementationType);
                if (interceptors.IsEmpty)
                {
                    _primaryDescriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                }
                else
                {
                    _primaryDescriptor = new ServiceDescriptor(serviceType, CreateOrGet, lifetime);
                    object CreateOrGet(IServiceProvider serviceProvider)
                    {
                        return factoryCache.GetTypeFactory(implementationType, interceptors).Invoke(serviceProvider);
                    }
                }
            }
        }

        public ServiceDescriptor[] AsServiceDescriptors() => _secondaryDescriptor == null
            ? new ServiceDescriptor[] { _primaryDescriptor }
            : new ServiceDescriptor[] { _primaryDescriptor, _secondaryDescriptor };
    }
}
