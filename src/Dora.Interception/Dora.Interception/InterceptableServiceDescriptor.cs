using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    public interface IInterceptableServiceDescriptor
    {
        Type TargetType { get; }
    }

    public sealed class InterceptableServiceDescriptor : ServiceDescriptor, IInterceptableServiceDescriptor
    {
        private readonly Type _targetType;

        public InterceptableServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
            : base(serviceType, GetImplementationFactory(serviceType, implementationType), lifetime)
        {
            if (serviceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Open generic type (generic type definition) is not support", nameof(serviceType));
            }
            _targetType = implementationType;
        }
        Type IInterceptableServiceDescriptor.TargetType => _targetType;

        private static Func<IServiceProvider, object> GetImplementationFactory(Type serviceType, Type implementationType)
        {
            return serviceProvider =>
            {
                var interceptorResolver = serviceProvider.GetRequiredService<IInterceptorResolver>();
                var codeGeneratorFactory = serviceProvider.GetRequiredService<ICodeGeneratorFactory>();
                var factoryCache = serviceProvider.GetRequiredService<IInterceptableProxyFactoryCache>();
                if (serviceType.IsInterface)
                {
                    var interceptors = interceptorResolver.GetInterceptors(serviceType, implementationType);
                    if (interceptors.IsEmpty)
                    {
                        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
                    }
                    else
                    {
                        var target = ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
                        return factoryCache.GetInstanceFactory(serviceType, implementationType).Invoke(target);
                    }
                }
                else
                {
                    var interceptors = interceptorResolver.GetInterceptors(implementationType);
                    if (interceptors.IsEmpty)
                    {
                        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
                    }
                    else
                    {
                        return factoryCache.GetTypeFactory(implementationType).Invoke(serviceProvider);
                    }
                }
            };
        }
    }

    public sealed class GenericInterceptableServiceDescriptor : ServiceDescriptor, IInterceptableServiceDescriptor
    {
        private readonly Type _targetType;
        public GenericInterceptableServiceDescriptor(
            ICodeGeneratorFactory codeGeneratorFactory,
            IInterceptorResolver  interceptorResolver,
            Type serviceType, Type implementationType, ServiceLifetime lifetime)
           : base(serviceType, GetInterceptableProxyType(codeGeneratorFactory, interceptorResolver, serviceType, implementationType), lifetime)
        {
            if (!serviceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Non-open-generic type (generic type definition) is not support", nameof(serviceType));
            }
            _targetType = implementationType;
        }

        Type IInterceptableServiceDescriptor.TargetType => _targetType;

        private static Type GetInterceptableProxyType(
            ICodeGeneratorFactory codeGeneratorFactory,
            IInterceptorResolver interceptorResolver,
            Type serviceType, 
            Type implementationType)
        {
            var interceptors = serviceType.IsInterface
                ? interceptorResolver.GetInterceptors(serviceType, implementationType)
                : interceptorResolver.GetInterceptors(implementationType);
            var codeGenerator = codeGeneratorFactory.Create();
            var context = new CodeGenerationContext(serviceType, implementationType, interceptors);
            var proxyType = codeGenerator.GenerateInterceptableProxyClass(context);
            return proxyType;
        }
    }
}
