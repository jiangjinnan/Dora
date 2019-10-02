using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace App
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            Environment = environment;
        }

        public IHostingEnvironment Environment { get; }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<ISystemClock, SystemClock>()
                .AddMvc();

            return services.BuildInterceptableServiceProvider(builder => builder
                .AddFilters(filters => filters.Add(new CacheReturnValueAttribute(), method => method.DeclaringType == typeof(SystemClock) && method.Name == "GetCurrentTime1")));

            //return services.BuildInterceptableServiceProvider(builder => builder.AddPolicy("Interception.dora", fileBuilder => fileBuilder
            //    .AddImports("App")
            //    .AddReferences(typeof(Startup).Assembly)
            //    .SetFileProvider(Environment.ContentRootFileProvider)));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}

