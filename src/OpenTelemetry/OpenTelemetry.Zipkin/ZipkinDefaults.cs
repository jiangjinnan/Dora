using System.Text.Json;

namespace Dora.OpenTelemetry.Zipkin
{
    public static class ZipkinDefaults
    {
        public const string EndpointEnvironmentVariable = "OTEL_EXPORTER_ZIPKIN_ENDPOINT";
        public static readonly Uri DefaultZipkinEndpoint = new ("http://localhost:9411/api/v2/spans");
        public const string HttpClientName = "Zipkin";

        internal static class SpanPropertyNames
        {
            public static readonly JsonEncodedText TraceId = JsonEncodedText.Encode("traceId");

            public static readonly JsonEncodedText Name = JsonEncodedText.Encode("name");

            public static readonly JsonEncodedText ParentId = JsonEncodedText.Encode("parentId");

            public static readonly JsonEncodedText Id = JsonEncodedText.Encode("id");

            public static readonly JsonEncodedText Kind = JsonEncodedText.Encode("kind");

            public static readonly JsonEncodedText Timestamp = JsonEncodedText.Encode("timestamp");

            public static readonly JsonEncodedText Duration = JsonEncodedText.Encode("duration");

            public static readonly JsonEncodedText Debug = JsonEncodedText.Encode("debug");

            public static readonly JsonEncodedText Shared = JsonEncodedText.Encode("shared");

            public static readonly JsonEncodedText LocalEndpoint = JsonEncodedText.Encode("localEndpoint");

            public static readonly JsonEncodedText RemoteEndpoint = JsonEncodedText.Encode("remoteEndpoint");

            public static readonly JsonEncodedText Annotations = JsonEncodedText.Encode("annotations");

            public static readonly JsonEncodedText Value = JsonEncodedText.Encode("value");

            public static readonly JsonEncodedText Tags = JsonEncodedText.Encode("tags");

            public static readonly JsonEncodedText ServiceName = JsonEncodedText.Encode("serviceName");

            public static readonly JsonEncodedText Ipv4 = JsonEncodedText.Encode("ipv4");

            public static readonly JsonEncodedText Ipv6 = JsonEncodedText.Encode("ipv6");

            public static readonly JsonEncodedText Port = JsonEncodedText.Encode("port");
        }
    }
}
