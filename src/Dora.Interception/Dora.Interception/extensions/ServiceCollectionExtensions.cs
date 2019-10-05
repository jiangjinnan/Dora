using Dora;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            services.AddInterception(configure);
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

        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            services.AddHttpContextAccessor();
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
    }
}
