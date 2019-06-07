using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dora.Interception;

namespace App
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<ISystemClock, SystemClock>()
                .AddMvc();
            return services.BuildInterceptableServiceProvider(builder => builder.AddPolicy(policyBuilder =>
                  policyBuilder.For<CacheReturnValueAttribute>(1, cache => cache.To<SystemClock>(clock => clock.IncludeMethod(it => it.GetCurrentTime(default))))));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}

