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
                .AddScoped<ISystemClock, SystemClock>()
                .AddMvc();
            return services.BuildInterceptableServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}

