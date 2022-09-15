using Dora.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Reflection;

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
            builder.Services.AddOptions<TracingOptions>().Configure<IServiceProvider>((options, provider) => {
                var appName = provider.GetRequiredService<IHostingEnvironment>().ApplicationName;
                var assemblyName = new AssemblyName(appName);
                var assembly = Assembly.Load(assemblyName);
                var version = assembly?.GetName()?.Version?.ToString();
                options.ServiceInstance = new ServiceInstance(appName, null, version, null);
            });
            return builder;            
        }
    }
}
