using Dora;
using Dora.Interception;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dora.DynamicProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Define some extension methods to register interception based services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            configure?.Invoke(new InterceptionBuilder(services));
             services
               .AddScoped(typeof(IInterceptable<>), typeof(Interceptable<>))
               .AddScoped<IInterceptorChainBuilder, InterceptorChainBuilder>() 
               .AddScoped<IInterceptorCollector, InterceptorCollector>()
               .AddScoped<IInterceptingProxyFactory, InterceptingProxyFactory>()
               .AddScoped< IInstanceDynamicProxyGenerator, InterfaceDynamicProxyGenerator>();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceProvider BuilderInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            return BuilderInterceptableServiceProvider(services, false, configure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceProvider BuilderInterceptableServiceProvider(this IServiceCollection services, bool validateScopes, Action<InterceptionBuilder> configure = null)
        {
            var options = new ServiceProviderOptions { ValidateScopes = validateScopes };
            services.AddInterception(configure);
            var proxyFactory = services.BuildServiceProvider().GetRequiredService<IInterceptingProxyFactory>();
            return new ServiceProvider2(services, options , proxyFactory);
        }
    }
}
