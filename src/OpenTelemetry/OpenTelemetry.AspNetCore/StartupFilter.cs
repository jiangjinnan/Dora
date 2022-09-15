using Dora.OpenTelemetry.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dora.OpenTelemetry.AspNetCore
{
    internal class StartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app => {
                app.ApplicationServices.GetRequiredService<IActivitySourceProvider>().GetActivitySource();                
                next(app);
            };
        }
    }
}
