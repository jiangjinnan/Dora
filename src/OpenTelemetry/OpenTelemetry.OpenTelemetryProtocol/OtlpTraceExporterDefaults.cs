namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    public static class OtlpTraceExporterDefaults
    {
        public const string HttpClientName = "Otlp";
        public static class EnvironmentVariables
        {
            public const string Endpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
            public const string Headers = "OTEL_EXPORTER_OTLP_HEADERS";
            public const string Timeout = "OTEL_EXPORTER_OTLP_TIMEOUT";
            public const string Protocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
        }

        public static class DefaultEndpoints
        {
            public const string Grpc = "http://localhost:4317";
            public const string Http = "http://localhost:4318";
        }
    }
}
