using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddScoped<ScopedService>()
                .AddScoped<ISystemClock, SystemClock>()
                .AddMvc();
            return services.BuildInterceptableServiceProvider(true);
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseDeveloperExceptionPage()
                .UseMvc();
        }
    }
}

