using Dora.OpenTelemetry.Tracing;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Opentelemetry.Proto.Collector.Trace.V1;
using static Opentelemetry.Proto.Collector.Trace.V1.TraceService;

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    internal class TracePayloadDeliverer : IPayloadDeliverer<ExportTraceServiceRequest>,IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly TraceServiceClient _client;
        private readonly TimeSpan _timeout;

        public TracePayloadDeliverer(IOptions<OtlpTraceExporterOptions> optionsAccessor)
        {
            var options = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value;
            _timeout = options.SendTimeout;
            //TODO
            _channel = GrpcChannel.ForAddress(options.Endpoint);
            _client = new TraceServiceClient(_channel);
        }

        public void Dispose()=>_channel.ShutdownAsync().Wait();
        public void Send(ExportTraceServiceRequest payload)
        {
            using (new SuppressScope())
            {
                var source = new CancellationTokenSource();
                source.CancelAfter(_timeout);
                _client.Export(payload, null, null, source.Token);
            }
        }
    }
}
