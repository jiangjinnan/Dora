using Dora;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines <see cref="IServiceCollection"/> specific extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Builds interceptable <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="services">The contract for a collection of service descriptors.</param>
        /// <param name="configure">The <see cref="Action{InterceptionBuilder}"/> used to perform more service registrations.</param>
        /// <returns>The current <see cref="IServiceCollection"/>.</returns>
        public static IServiceProvider BuildInterceptableServiceProvider(
            this IServiceCollection services, 
            Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            services.TryAddInterception(configure);
            var provider = services.BuildServiceProvider();
            var factoryCache = provider.GetRequiredService<IInterceptableProxyFactoryCache>();
            var resolver = provider.GetRequiredService<IInterceptorResolver>();
            var codeGeneratorFactory = provider.GetRequiredService<ICodeGeneratorFactory>();

            IServiceCollection newServices = new ServiceCollection();
            foreach (var service in services)
            {
                foreach (var newService in new ServiceDescriptorConverter(service, resolver, factoryCache, codeGeneratorFactory).AsServiceDescriptors())
                {
                    newServices.Add(newService);
                }
            }
            return newServices.BuildServiceProvider();
        }

        /// <summary>
        /// Add interception based services. 
        /// </summary>
        /// <param name="services">The contract for a collection of service descriptors.</param> 
        /// <param name="configure">The <see cref="Action{InterceptionBuilder}"/> used to perform more service registrations.</param>
        /// <returns>The current <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            if (services.Any(it => it.ServiceType == typeof(IDuplicate)))
            {
                throw new InvalidOperationException("Duplicate invocation to AddInterception method.");
            }
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IDuplicate,Duplicate>();
            services.TryAddSingleton<ICodeGeneratorFactory, CodeGeneratorFactory>();
            services.TryAddSingleton<IInterceptorChainBuilder, InterceptorChainBuilder>();
            services.TryAddSingleton<IInterceptableProxyFactoryCache, InterceptableProxyFactoryCache>();

            var builder = new InterceptionBuilder(services);
            configure?.Invoke(builder);
            services.TryAddSingleton<IInterceptorResolver>(provider =>
            {
                var chainBuilder = provider.GetRequiredService<IInterceptorChainBuilder>();
                var providerResolvers = builder.InterceptorProviderResolvers;
                if (!providerResolvers.OfType<AttributeInterceptorProviderResolver>().Any())
                {
                    providerResolvers.Add(nameof(AttributeInterceptorProviderResolver), new AttributeInterceptorProviderResolver());
                }
                return new InterceptorResolver(chainBuilder, providerResolvers);
            });
            return services;
        }

        /// <summary>
        /// Try to add interception based services is missing. 
        /// </summary>
        /// <param name="services">The contract for a collection of service descriptors.</param> 
        /// <param name="configure">The <see cref="Action{InterceptionBuilder}"/> used to perform more service registrations.</param>
        /// <returns>The current <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection TryAddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            if (!services.Any(it => it.ServiceType == typeof(IDuplicate)))
            {
                services.AddInterception(configure);
            }
            return services;
        }
        private interface IDuplicate { }
        private class Duplicate : IDuplicate { }
    }
}
