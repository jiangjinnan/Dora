using Dora.Interception.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, Action<InterceptionBuilder> setup = null)
        {
            Action<InterceptionBuilder> setup2 = builder => {
                setup?.Invoke(builder);
                builder.Services.Replace(ServiceDescriptor.Singleton<IServiceProviderAccessor, RequestServiceProviderAccessor>());
            };
            hostBuilder.UseServiceProviderFactory(new InterceptionServiceProviderFactory(setup2));
            return hostBuilder;
        }
    }
}
