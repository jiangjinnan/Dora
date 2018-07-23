using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    public static class ServiceCollectionExtensions
    {        
        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, Action<IInterceptionPolicyBuilder> configure)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var registrationBuilder = new InterceptionPolicyBuilder(serviceProvider);
            configure.Invoke(registrationBuilder);              
            var resolver = new ExpressionInterceptorProviderResolver(registrationBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(ExpressionInterceptorProviderResolver), resolver);
            return builder;
        }
    }
}
