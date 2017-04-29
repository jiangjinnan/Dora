using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<ISystomClock, SystomClock>()
                .AddMvc();
            return services.BuilderInterceptableServiceProvider(builder => builder.SetDynamicProxyFactory());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
