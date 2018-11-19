using Dora;
using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;
using System.Linq;
using Dora.Interception.Internal;

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
            => services.AddInterceptionCore(configure, true);

        private static IServiceCollection AddInterceptionCore(this IServiceCollection services, Action<InterceptionBuilder> configure, bool overrideExistingRegistrations)
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
            services.AddSingleton<IInterceptorResolver>(_ => new InterceptorResolver(_.GetRequiredService<IInterceptorChainBuilder>(), builder.InterceptorProviderResolvers));

            var provider = services.BuildServiceProvider();
            var resolver = provider.GetRequiredService<IInterceptorResolver>();
            var proxyFactory = provider.GetRequiredService<IInterceptingProxyFactory>();
            if (overrideExistingRegistrations)
            {
                OverrideRegistrations(services, resolver, proxyFactory);
                services.AddSingleton<IInterceptableServiceProviderIndicator>(new InterceptableServiceProviderIndicator(false));
            }
            else
            {
                services.AddSingleton<IInterceptableServiceProviderIndicator>(new InterceptableServiceProviderIndicator(true));
            }
            return services;
        }

        private static void OverrideRegistrations(IServiceCollection services, IInterceptorResolver resolver, IInterceptingProxyFactory proxyFactory)
        {
            services.TryAddSingleton<ServiceOverrideIndicator, ServiceOverrideIndicator>();
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
                var newDescriptor = new ServiceDescriptor(serviceType, _ => proxyFactory.Create(item.ServiceDescriptor.ImplementationType), item.ServiceDescriptor.Lifetime);
                services.Add(newDescriptor);               
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
            if (services.Any(it => it.ServiceType == typeof(ServiceOverrideIndicator)))
            {
                throw new InvalidOperationException("AddInterception method cannot be called if BuildInterceptableServiceProvider method is called to create interceptable service provider.");
            }
            var options = new ServiceProviderOptions { ValidateScopes = validateScopes };
            services.AddInterceptionCore(configure, false);
            var proxyFactory = services.BuildServiceProvider().GetRequiredService<IInterceptingProxyFactory>();
            return new InterceptableServiceProvider(services, options, proxyFactory);
        }

        private class ServiceOverrideIndicator { }
    }
}
