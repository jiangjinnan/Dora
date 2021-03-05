using Dora.Interception;
using Dora.Interception.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, Action<InterceptionBuilder> setup = null)
        {
            hostBuilder.UseServiceProviderFactory(new InterceptionServiceProviderFactory(Configure));
            return hostBuilder;

            void Configure(InterceptionBuilder builder)
            {
                setup?.Invoke(builder);
                builder.Services.AddHttpContextAccessor();
                builder.Services.Replace(ServiceDescriptor.Singleton<IServiceProviderAccessor, RequestServiceProviderAccessor>());
            }
        }
    }
}
