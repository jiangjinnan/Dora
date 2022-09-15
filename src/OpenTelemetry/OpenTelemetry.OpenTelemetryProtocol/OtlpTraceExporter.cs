using Dora.OpenTelemetry.Tracing;
using Opentelemetry.Proto.Collector.Trace.V1;
using Opentelemetry.Proto.Trace.V1;
using System.Diagnostics;

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    public class OtlpTraceExporter : IActivityExporter
    {
        private readonly IPayloadGenerator<ExportTraceServiceRequest> _generator;
        private readonly IPayloadDeliverer<ExportTraceServiceRequest> _deliverer;

        public OtlpTraceExporter(IPayloadGenerator<ExportTraceServiceRequest> generator, IPayloadDeliverer<ExportTraceServiceRequest> deliverer)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _deliverer = deliverer ?? throw new ArgumentNullException(nameof(deliverer));
        }

        public void Export(IEnumerable<Activity> activities)
        {
            var payload = _generator.Generate(activities);
            _deliverer.Send(payload);
        }
    }
}
