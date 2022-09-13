namespace Dora.OpenTelemetry.Zipkin
{
    public sealed class ZipkinExporterOptions
    {
        public Uri Endpoint { get; set; } = ZipkinDefaults.DefaultZipkinEndpoint;
        public IList<Action<HttpClient>> HttpClientSetups = new List<Action<HttpClient>>();
        public ZipkinExporterOptions()
        {
            var endpoint = Environment.GetEnvironmentVariable(ZipkinDefaults.EndpointEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                Endpoint = new Uri(endpoint);
            }
        }

        public ZipkinExporterOptions ConfigureHttpClient(Action<HttpClient> configure)
        {
            HttpClientSetups.Add(configure?? throw new ArgumentNullException(nameof(configure)));
            return this;
        }
    }
}
