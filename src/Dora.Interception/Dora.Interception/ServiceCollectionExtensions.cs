using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterception<TProxyFactory, TInterceptorChainBuilder>(this IServiceCollection services) 
            where TProxyFactory : class, IProxyFactory
            where TInterceptorChainBuilder: class, IInterceptorChainBuilder
        {
            return services
                .AddScoped(typeof(IInterceptable<>), typeof(Interceptable<>))
                .AddScoped<IProxyFactory, TProxyFactory>()
                .AddScoped<IInterceptorChainBuilder, TInterceptorChainBuilder>();
        }

        public static IServiceCollection AddInterception<TProxyFactory>(this IServiceCollection services)
           where TProxyFactory : class, IProxyFactory
        {
            return services.AddInterception<TProxyFactory, InterceptorChainBuilder>();
        }
        public static void  ToInterceptable(this IServiceCollection services)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IServiceCollection newServices = GetInterceptableServices(serviceProvider, services);
            for (int i = 0; i < services.Count; i++)
            {
                services[i] = newServices[i];
            }
        }
        private static IServiceCollection GetInterceptableServices(IServiceProvider serviceProvider,IServiceCollection services)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            foreach (var service in services)
            {
                bool isOpneGeneric = service.ServiceType.GetTypeInfo().IsGenericTypeDefinition;
                Func<IServiceProvider, object> factory = _ => Wrap(serviceProvider, service.ServiceType);
                switch (service.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        {
                            if (isOpneGeneric)
                            {
                                serviceCollection.AddScoped(service.ServiceType, service.ImplementationType);
                            }
                            else
                            {
                                serviceCollection.AddScoped(service.ServiceType, factory);
                            }
                            break;
                        }
                    case ServiceLifetime.Singleton:
                        {
                            if (isOpneGeneric)
                            {
                                serviceCollection.AddSingleton(service.ServiceType, service.ImplementationType);
                            }
                            else
                            {
                                serviceCollection.AddSingleton(service.ServiceType, factory);
                            }
                            break;
                        }
                    case ServiceLifetime.Transient:
                        {
                            if (isOpneGeneric)
                            {
                                serviceCollection.AddTransient(service.ServiceType, service.ImplementationType);
                            }
                            else
                            {
                                serviceCollection.AddTransient(service.ServiceType, factory);
                            }
                            break;
                        }
                }
            }
            return serviceCollection;
        }
        private static object Wrap(IServiceProvider serviceProvider,Type serviceType)
        {
            object target = serviceProvider.GetService(serviceType);
            if (target != null && target is IInterceptable)
            {
                IProxyFactory proxyFactory = serviceProvider.GetRequiredService<IProxyFactory>();
                return proxyFactory.CreateProxy(serviceType, target);
            }
            return target;
        }
    }
}
