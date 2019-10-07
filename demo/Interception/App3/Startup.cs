using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddInterception()
                .AddSingleton<ISystemClock, SystemClock>()
                .AddRouting()
                .AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
