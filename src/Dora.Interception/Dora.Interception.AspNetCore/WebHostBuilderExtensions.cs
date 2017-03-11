using Dora.Interception.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseInterception(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(svcs => svcs.AddTransient<IStartupFilter, InterceptionServiceProviderStartupFilter>());
        }
    }
}
