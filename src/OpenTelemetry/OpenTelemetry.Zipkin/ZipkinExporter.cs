using Dora.OpenTelemetry.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Zipkin
{
    internal class ZipkinExporter : IActivityExporter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _endpoint;
        private readonly Action<ILogger, Exception> _logError;
        private readonly ILogger _logger;
        private readonly IZipkinSpanWriter _writer;
        private readonly TimeSpan _timeout;

        public ZipkinExporter(IHttpClientFactory httpClientFactory, IZipkinSpanWriter writer, IOptions<ZipkinExporterOptions> optionsAccessor, ILogger<ZipkinExporter> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _endpoint = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value.Endpoint;
            _logError = LoggerMessage.Define(LogLevel.Error, 0, "Failed to export activities to zipkin.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeout = optionsAccessor.Value.SendTimeout;
        }

        public void Export(IEnumerable<Activity> activities)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
                {
                    Content = new ZipkinTraceContent(_writer, activities)
                };
                var httpCient = _httpClientFactory.CreateClient(ZipkinDefaults.HttpClientName);
                using (new SuppressScope())
                {
                    var source = new CancellationTokenSource();
                    source.CancelAfter(_timeout);
                    httpCient.Send(request, source.Token).EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logError(_logger, ex);
            }
        }
    }
}
