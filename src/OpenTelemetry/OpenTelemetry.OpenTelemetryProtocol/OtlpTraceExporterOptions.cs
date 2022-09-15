namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    public class OtlpTraceExporterOptions
    {
        public OtlpExportProtocol Protocol { get; set; } = OtlpExportProtocol.Grpc;
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public Uri Endpoint { get; set; } 
        public Action<HttpClient> HttpClientConfigurator { get; internal set; }
        public OtlpTraceExporterOptions()
        {
            var endpoint = Environment.GetEnvironmentVariable(OtlpTraceExporterDefaults.EnvironmentVariables.Endpoint);
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                Endpoint = new Uri(endpoint);
            }
            else
            {
                Endpoint = Protocol == OtlpExportProtocol.Grpc ? new Uri(OtlpTraceExporterDefaults.DefaultEndpoints.Grpc) : new Uri(OtlpTraceExporterDefaults.DefaultEndpoints.Http);
            }
            var sendTimeout = Environment.GetEnvironmentVariable(OtlpTraceExporterDefaults.EnvironmentVariables.Timeout);
            if (!string.IsNullOrWhiteSpace(sendTimeout) && TimeSpan.TryParse(sendTimeout, out var timeout ))
            {
                SendTimeout = timeout;
            }
            HttpClientConfigurator = httpClinet => httpClinet.Timeout = SendTimeout;
        }

        public OtlpTraceExporterOptions ConfigureHttpClient(Func<Action<HttpClient>, Action<HttpClient>> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            HttpClientConfigurator = configure(HttpClientConfigurator);
            return this;
        }

        public OtlpTraceExporterOptions SetSendTimeout(TimeSpan timeout)
            => ConfigureHttpClient(configure => httpClient => { configure(httpClient); httpClient.Timeout = timeout; });
    }
}
