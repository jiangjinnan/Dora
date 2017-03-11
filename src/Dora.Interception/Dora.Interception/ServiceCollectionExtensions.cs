using Dora;
using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Define some extension methods to register interception based services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register interception based services.
        /// </summary>
        /// <typeparam name="TProxyFactory">The type of proxy factory.</typeparam>
        /// <typeparam name="TInterceptorChainBuilder">The type of interceptor chain builder.</typeparam>
        /// <param name="services">The service collection in which the service registrations are added.</param>
        /// <returns>The service collection with the interception based service registrations.</returns>
        public static IServiceCollection AddInterception<TProxyFactory, TInterceptorChainBuilder>(this IServiceCollection services) 
            where TProxyFactory : class, IProxyFactory
            where TInterceptorChainBuilder: class, IInterceptorChainBuilder
        {
            Guard.ArgumentNotNull(services, nameof(services));
            return services
                .AddScoped(typeof(IInterceptable<>), typeof(Interceptable<>))
                .AddScoped<IProxyFactory, TProxyFactory>()
                .AddScoped<IInterceptorChainBuilder, TInterceptorChainBuilder>();
        }

        /// <summary>
        /// Register interception based services. The <see cref="InterceptorChainBuilder"/> is used to builder interceptor chain.
        /// </summary>
        /// <typeparam name="TProxyFactory">The type of proxy factory.</typeparam>
        /// <param name="services">The service collection in which the service registrations are added.</param>
        /// <returns>The service collection with the interception based service registrations.</returns>
        public static IServiceCollection AddInterception<TProxyFactory>(this IServiceCollection services)
           where TProxyFactory : class, IProxyFactory
        {
            return services.AddInterception<TProxyFactory, InterceptorChainBuilder>();
        }
    }
}
