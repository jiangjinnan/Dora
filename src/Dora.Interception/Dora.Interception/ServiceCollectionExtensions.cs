using Dora;
using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Define some extension methods to register interception based services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the interception based service registrations.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> in which the service registrations are added.</param>
        /// <param name="configure">A <see cref="Action{InterceptionBuilder}"/> to perform other configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> with added service registration.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        { 
            Guard.ArgumentNotNull(services, nameof(services));
            services.TryAddTransient(typeof(IInterceptable<>), typeof(Interceptable<>));
            services.TryAddSingleton<IInterceptorChainBuilder, InterceptorChainBuilder>();    
            services.TryAddSingleton<IInterceptingProxyFactory, InterceptingProxyFactory>();
            services.TryAddSingleton<IInstanceDynamicProxyGenerator, InterfaceDynamicProxyGenerator>();
            services.TryAddSingleton<ITypeDynamicProxyGenerator, VirtualMethodDynamicProxyGenerator>();
            services.TryAddSingleton<IDynamicProxyFactoryCache>(new DynamicProxyFactoryCache());

            var builder = new InterceptionBuilder(services);   
            configure?.Invoke(builder);
            services.AddSingleton<IInterceptorResolver>(_=>new InterceptorResolver(_.GetRequiredService<IInterceptorChainBuilder>() , builder.InterceptorProviderResolvers));

            var resolver = services.BuildServiceProvider().GetRequiredService<IInterceptorResolver>();
            AddVirtualMethodServices(services, resolver);
            return services; 
        }

        private static void AddVirtualMethodServices(IServiceCollection services, IInterceptorResolver resolver)
        {
            var result = (from it in services
                               let implType = it.ImplementationType
                               let interceptors = implType == null
                                ? null
                                : resolver.GetInterceptors(it.ImplementationType)
                               where !it.ServiceType.IsInterface &&
                                it.ImplementationFactory == null &&
                                it.ImplementationInstance == null &&
                                it.ImplementationType != null &&
                                !interceptors.IsEmpty
                               select new { ServiceDescriptor = it, Interceptors = interceptors } )
                               .ToArray();

            Array.ForEach(result.Select(it => it.ServiceDescriptor).ToArray(), it => services.Remove(it));
            foreach (var item in result.ToArray())
            {
                var serviceType = item.ServiceDescriptor.ServiceType;
                var proxyType = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(item.ServiceDescriptor.ImplementationType, item.Interceptors).GenerateProxyType();
                switch (item.ServiceDescriptor.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        {
                            services.AddScoped(serviceType, provider => {
                                var proxy = ActivatorUtilities.CreateInstance(provider, proxyType);
                                ((IInterceptorsInitializer)proxy).SetInterceptors(item.Interceptors);
                                return proxy;
                            });
                            break;
                        }
                    case ServiceLifetime.Singleton:
                        {
                            services.AddSingleton(serviceType, provider => {
                                var proxy = ActivatorUtilities.CreateInstance(provider, proxyType);
                                ((IInterceptorsInitializer)proxy).SetInterceptors(item.Interceptors);
                                return proxy;
                            });
                            break;
                        }
                    case ServiceLifetime.Transient:
                        {
                            services.AddTransient(serviceType, provider => {
                                var proxy = ActivatorUtilities.CreateInstance(provider, proxyType);
                                ((IInterceptorsInitializer)proxy).SetInterceptors(item.Interceptors);
                                return proxy;
                            });
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Builders the interceptable service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the service provider is built based on.</param>
        /// <param name="configure">A <see cref="Action{InterceptionBuilder}"/> to perform other configuration.</param>
        /// <returns>The interceptable service provider.</returns>  
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            return BuildInterceptableServiceProvider(services, false, configure);
        }

        //public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, string policyFilePath, Assembly[] references, string[] imports)
        //{
        //    return services.BuildInterceptableServiceProvider(_ => _.AddPolicy(policyFilePath, references, imports));
        //} 


        /// <summary>
        /// Builders the interceptable service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the service provider is built based on.</param>
        /// <param name="validateScopes">if set to <c>true</c> [validate scopes].</param>
        /// <param name="configure">The configure.</param>
        /// <returns>The interceptable service provider.</returns>    
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, bool validateScopes, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            var options = new ServiceProviderOptions { ValidateScopes = validateScopes };
            services.AddInterception(configure);
            var proxyFactory = services.BuildServiceProvider().GetRequiredService<IInterceptingProxyFactory>();
            return new InterceptableServiceProvider(services, options, proxyFactory);
        }
    }
}
