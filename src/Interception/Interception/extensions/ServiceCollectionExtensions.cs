using Dora.Interception;
using Dora.Primitives;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> setup = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            services.TryAddSingleton<IInterceptorBuilder, InterceptorBuilder>();
            services.TryAddSingleton<IInterceptorProvider, InterceptorProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptableProxyGenerator, InterfaceProxyGenerator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptableProxyGenerator, VirtualMethodProxyGenerator>());
            services.TryAddSingleton<IServiceProviderAccessor, ServiceProviderAccessor>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorRegistrationProvider, AttributeInterceptorRegistrationProvider>());

            if (setup != null)
            {
                var builder = new InterceptionBuilder(services);
                setup(builder);
            }
            return services;
        }

        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder> setup = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            return new InterceptionContainer(services.AddInterception(setup)).BuildServiceProvider();
        }
    }
}