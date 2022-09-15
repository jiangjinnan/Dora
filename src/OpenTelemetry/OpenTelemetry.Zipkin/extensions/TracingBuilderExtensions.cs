using Dora.OpenTelemetry.Zipkin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dora.OpenTelemetry.Tracing
{
    public static class TracingBuilderExtensions
    {
        public static TracingBuilder ExportToZipkin(this TracingBuilder builder, Action<ZipkinExporterOptions>? setup = null)
        { 
            if(builder == null) throw new ArgumentNullException(nameof(builder));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityExporter, ZipkinExporter>());
            builder.Services.TryAddSingleton<ILocalEndpointResolver, LocalEndpointResolver>();
            builder.Services.TryAddSingleton<IZipkinSpanWriter, ZipkinSpanWriter>();
            if (setup != null)
            {
                builder.Services.Configure(setup);
            }
            builder.Services.AddHttpClient(ZipkinDefaults.HttpClientName, (provider, httpClient) =>
            {
                var options = provider.GetRequiredService<IOptions<ZipkinExporterOptions>>().Value;
                options.HttpClientConfigurator(httpClient);
            });
            return builder;

        }
    }
}
