using Dora.OpenTelemetry.OpenTelemetryProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Opentelemetry.Proto.Collector.Trace.V1;

namespace Dora.OpenTelemetry.Tracing
{
    public static class TracingBuilderExtensions
    {
        public static TracingBuilder ExportToOtlpCollector(this TracingBuilder builder, Action<OtlpTraceExporterOptions>? setup = null)
        { 
            if(builder == null) throw new ArgumentNullException(nameof(builder));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityExporter, OtlpTraceExporter>());
            builder.Services.TryAddSingleton<IPayloadGenerator<ExportTraceServiceRequest>, TracePayloadGenerator>();
            builder.Services.TryAddSingleton<IPayloadDeliverer<ExportTraceServiceRequest>, TracePayloadDeliverer>();
            if (setup != null)
            {
                builder.Services.Configure(setup);
            }
            builder.Services.AddHttpClient(OtlpTraceExporterDefaults.HttpClientName, (provider, httpClient) =>
            {
                var options = provider.GetRequiredService<IOptions<OtlpTraceExporterOptions>>().Value;
                options.HttpClientConfigurator(httpClient);
            });
            return builder;
        }
    }
}
