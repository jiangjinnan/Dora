using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        public class Startup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services
                    .AddScoped<ISystomClock, SystomClock>()
                    .AddScoped<ITimeProvider, TimeProvider>()
                    .Configure<MemoryCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2))
                    .AddMvc();
                 return services.BuilderInterceptableServiceProvider(builder => builder.SetDynamicProxyFactory());
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseMvc();
            }
        }
    }
}
