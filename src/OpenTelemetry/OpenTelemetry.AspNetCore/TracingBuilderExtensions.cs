using Dora.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public static class TracingBuilderExtensions
    {
        public static TracingBuilder InstrumentAspNetCore(this TracingBuilder builder)
        {
            builder.TryAddInstrumentation<AspNetCoreInstrumentation>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, StartupFilter>());
            var services = builder.Services.Where(it => it.ServiceType == typeof(ActivitySource)).ToArray();
            if (services.Length > 1)
            {
                var service = services.SingleOrDefault(it => it.ImplementationFactory == ActivitySourceProvider.ActivitySourceFactory);
                if (service is not null)
                { 
                    builder.Services.Remove(service);
                }
            }
            //var activitySourceService = builder.Services.SingleOrDefault(it=>it.ServiceType == typeof(ActivitySource) && it.ImplementationType is null);
            //if (activitySourceService is not null)
            //{ 
            //    builder.Services.Remove(activitySourceService);
            //}
            return builder;
        }
    }
}
