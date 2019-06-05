using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionServiceExtensions
    {
        private static Dictionary<IServiceCollection, IInterceptorResolver> _resolvers = new Dictionary<IServiceCollection, IInterceptorResolver>();
        private static Dictionary<IServiceCollection, IInstanceDynamicProxyGenerator> _instanceDynamicProxyGenerators = new Dictionary<IServiceCollection, IInstanceDynamicProxyGenerator>();
        private static Dictionary<IServiceCollection, ITypeDynamicProxyGenerator> _typeDynamicProxyGenerators = new Dictionary<IServiceCollection, ITypeDynamicProxyGenerator>();

        private static IInterceptorResolver GetResolver(IServiceCollection collection)
        {
            return _resolvers.TryGetValue(collection, out var resolver)
                ? resolver
                : _resolvers[collection] = collection.BuildServiceProvider().GetRequiredService<IInterceptorResolver>();
        }
        private static IInstanceDynamicProxyGenerator GetInstanceProxyGenerator(IServiceCollection collection)
        {
            return _instanceDynamicProxyGenerators.TryGetValue(collection, out var generator)
                ? generator
                : _instanceDynamicProxyGenerators[collection] = collection.BuildServiceProvider().GetRequiredService<IInstanceDynamicProxyGenerator>();
        }

        private static ITypeDynamicProxyGenerator GetTypeProxyGenerator(IServiceCollection collection)
        {
            return _typeDynamicProxyGenerators.TryGetValue(collection, out var generator)
                ? generator
                : _typeDynamicProxyGenerators[collection] = collection.BuildServiceProvider().GetRequiredService<ITypeDynamicProxyGenerator>();
        }

        public static IServiceCollection AddInterceptable(this IServiceCollection collection, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (serviceType.IsInterface)
            {
                var interceptors = GetResolver(collection).GetInterceptors(serviceType, implementationType);
                object CreateProxy(IServiceProvider provider)
                {
                    var target = ActivatorUtilities.CreateInstance(provider, implementationType);
                    return GetInstanceProxyGenerator(collection).Wrap(serviceType, target, interceptors);
                }
                collection.Add(ServiceDescriptor.Describe(serviceType, CreateProxy, lifetime));
            }
            else
            {
                var interceptors = GetResolver(collection).GetInterceptors(implementationType);
                object CreateProxy(IServiceProvider provider)
                {
                    return GetTypeProxyGenerator(collection).Create(implementationType, interceptors, provider);
                }
                collection.Add(ServiceDescriptor.Describe(serviceType, CreateProxy, lifetime));
            }
            return collection;
        }
        public static IServiceCollection AddInterceptableScoped<TService>(this IServiceCollection services) where TService : class
        {
            return services.AddInterceptable(typeof(TService), typeof(TService), ServiceLifetime.Scoped);
        }
        public static IServiceCollection AddInterceptableScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            return services.AddInterceptable(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);
        }
        public static IServiceCollection AddInterceptableScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
        {
            if (!typeof(TService).IsInterface)
            {
                throw new InvalidOperationException($"{typeof(TService).Name}  is not an interface.");
            }

            var interceptors = GetResolver(services).GetInterceptors(typeof(TService), typeof(TImplementation));
            object CreateProxy(IServiceProvider provider)
            {
                var target = implementationFactory(provider);
                return GetInstanceProxyGenerator(services).Wrap(typeof(TService), target, interceptors);
            }
            services.Add(ServiceDescriptor.Describe(typeof(TService), CreateProxy,  ServiceLifetime.Scoped));
            return services;
        }
        //public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class;
        //public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType);
        //public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory);
        //public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Type implementationType);
        //public static IServiceCollection AddSingleton<TService>(this IServiceCollection services) where TService : class;
        //public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService;
        //public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance) where TService : class;
        //public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService;
        //public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class;
        //public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType);
        //public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory);
        //public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance);
        //public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Type implementationType);
        //public static IServiceCollection AddTransient<TService>(this IServiceCollection services) where TService : class;
        //public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService;
        //public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService;
        //public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class;
        //public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType);
        //public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory);
        //public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationType);
    }
}
