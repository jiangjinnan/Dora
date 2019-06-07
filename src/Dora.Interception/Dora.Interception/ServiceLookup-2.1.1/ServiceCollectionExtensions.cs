using Dora.DynamicProxy;
using Dora.Interception;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines extension methods to register interceptable services.
    /// </summary>
    public static class ServiceCollectionServiceExtensions
    {
        private static readonly Dictionary<IServiceCollection, object> _cache = new Dictionary<IServiceCollection, object>();
        private static (IInterceptorResolver Resolver, IInstanceDynamicProxyGenerator InstanceGenerator, ITypeDynamicProxyGenerator TypeGenerator) GetResolverAndClassGenerators(IServiceCollection services)
        {
            if (_cache.TryGetValue(services, out var value))
            {
                return ((IInterceptorResolver Resolver, IInstanceDynamicProxyGenerator InstanceGenerator, ITypeDynamicProxyGenerator TypeGenerator))value;
            }
            var provider = services.BuildServiceProvider();
            var resolver = provider.GetRequiredService<IInterceptorResolver>();
            var instanceGenerator = provider.GetRequiredService<IInstanceDynamicProxyGenerator>();
            var typeGenerator = provider.GetRequiredService<ITypeDynamicProxyGenerator>();
            (IInterceptorResolver Resolver, IInstanceDynamicProxyGenerator InstanceGenerator, ITypeDynamicProxyGenerator TypeGenerator) resolverAndGenerators = (resolver, instanceGenerator, typeGenerator);
            _cache[services] = resolverAndGenerators;
            return resolverAndGenerators;
        }

        private static IServiceCollection AddInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var (resolver, instanceGenerator, typeGenerator) = GetResolverAndClassGenerators(services);
            if (serviceType.IsInterface)
            {
                var interceptors = resolver.GetInterceptors(serviceType, implementationType);
                object CreateProxy(IServiceProvider provider)
                {
                    var target = ActivatorUtilities.CreateInstance(provider, implementationType);
                    return instanceGenerator.Wrap(serviceType, target, interceptors);
                }
                services.Add(ServiceDescriptor.Describe(serviceType, CreateProxy, lifetime));
            }
            else
            {
                var interceptors = resolver.GetInterceptors(implementationType);
                object CreateProxy(IServiceProvider provider)
                {
                    return typeGenerator.Create(implementationType, interceptors, provider);
                }
                services.Add(ServiceDescriptor.Describe(serviceType, CreateProxy, lifetime));
            }
            return services;
        }

        /// <summary>
        /// Adds an interceptable scoped service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableScoped<TService>(this IServiceCollection services) where TService : class
        {
            services.AddScoped<TService>();
            return services.AddInterceptable(typeof(TService), typeof(TService), ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds an interceptable scoped service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            return services.AddInterceptable(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds an interceptable scoped service of the type.
        /// </summary>
        /// <param name="serviceType">The type of the service to add.</param>
        /// <param name="implementationType">The type of the implementation to use.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableScoped(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            return services.AddInterceptable(serviceType, implementationType, ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds an interceptable singleton service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableSingleton<TService>(this IServiceCollection services) where TService : class
        {
            return services.AddInterceptable(typeof(TService), typeof(TService), ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds an interceptable singleton service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            return services.AddInterceptable(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds an interceptable singleton service of the type.
        /// </summary>
        /// <param name="serviceType">The type of the service to add.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableSingleton(this IServiceCollection services, Type serviceType)
        {
            return services.AddInterceptable(serviceType, serviceType, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds an interceptable singleton service of the type.
        /// </summary>
        /// <param name="serviceType">The type of the service to add.</param>
        /// <param name="implementationType">The type of the implementation to use.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            return services.AddInterceptable(serviceType, implementationType, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds an interceptable transient service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableTransient<TService>(this IServiceCollection services) where TService : class
        {
            return services.AddInterceptable(typeof(TService), typeof(TService), ServiceLifetime.Transient);
        }

        /// <summary>
        /// Adds an interceptable transient service of the type.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableTransient<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            return services.AddInterceptable(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);
        }

        /// <summary>
        /// Adds an interceptable transient service of the type.
        /// </summary>
        /// <param name="serviceType">The type of the service to add.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInterceptableTransient(this IServiceCollection services, Type serviceType)
        {
            return services.AddInterceptable(serviceType, serviceType, ServiceLifetime.Transient);
        }

        /// <summary>
        /// Adds an interceptable transient service of the type.
        /// </summary>
        /// <param name="serviceType">The type of the service to add.</param>
        /// <param name="implementationType">The type of the implementation to use.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns> A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            return services.AddInterceptable(serviceType, implementationType, ServiceLifetime.Transient);
        }
    }
}
