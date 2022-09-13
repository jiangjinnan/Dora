using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dora.OpenTelemetry.Tracing
{
    public static class TracingBuilderExtensions
    {
        public static TracingBuilder SetServiceInstance(this TracingBuilder builder, string name, string @namespace = "", string version = "", string instanceId = "")
        { 
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (name == null) throw new ArgumentNullException(nameof(name));
            builder.Services.Configure<TracingOptions>(options =>
            {
                var originalName = options.ServiceInstance?.Name;
                options.ServiceInstance = new ServiceInstance(name, version, instanceId, @namespace);
                if (originalName is not null)
                {
                    options.ActivitySourceNames.Remove(originalName);
                }
                options.ActivitySourceNames.Add(name);
            });
            return builder;
        }

        public static TracingBuilder AddConsoleExporter(this TracingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityExporter, ConsoleExporter>());
            return builder;
        }

        public static TracingBuilder TryAddInstrumentation<TInstrumentation>(this TracingBuilder builder) where TInstrumentation : class,IInstrumentation
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IInstrumentation, TInstrumentation>());
            return builder;
        }
    }
}
