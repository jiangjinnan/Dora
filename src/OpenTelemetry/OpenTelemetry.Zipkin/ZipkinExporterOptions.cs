namespace Dora.OpenTelemetry.Zipkin
{
    public sealed class ZipkinExporterOptions
    {
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public Uri Endpoint { get; set; } = ZipkinDefaults.DefaultZipkinEndpoint;
        public Action<HttpClient> HttpClientConfigurator { get; internal set; }
        public ZipkinExporterOptions()
        {
            var endpoint = Environment.GetEnvironmentVariable(ZipkinDefaults.EndpointEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                Endpoint = new Uri(endpoint);
            }
            HttpClientConfigurator = httpClinet => httpClinet.Timeout = SendTimeout;
        }

        public ZipkinExporterOptions ConfigureHttpClient(Func<Action<HttpClient>, Action<HttpClient>> configure)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));
            HttpClientConfigurator = configure(HttpClientConfigurator);
            return this;
        }

        public ZipkinExporterOptions SetSendTimeout(TimeSpan timeout) 
            => ConfigureHttpClient(configure => httpClient => { configure(httpClient); httpClient.Timeout = timeout; });
    }
}
