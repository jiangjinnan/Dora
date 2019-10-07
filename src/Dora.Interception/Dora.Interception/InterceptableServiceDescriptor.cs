using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a normal non-open-generic type based interceptable <see cref="ServiceDescriptor"/>.
    /// </summary>
    public sealed class InterceptableServiceDescriptor : ServiceDescriptor, IInterceptableServiceDescriptor
    {
        private readonly Type _targetType;

        /// <summary>
        /// Create a new <see cref="InterceptableServiceDescriptor"/>.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service registration.</param>
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

    
}
