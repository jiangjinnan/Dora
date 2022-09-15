using Dora.OpenTelemetry.Http;

namespace Dora.OpenTelemetry.Tracing
{
    public static class TracingBuilderExtensions
    {
        public static TracingBuilder InstrumentHttpClient(this TracingBuilder builder)
        {
            builder.TryAddInstrumentation<HttpClientInstrumentation>();
            return builder;
        }
    }
}
