using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Dora.OpenTelemetry.Zipkin
{
    public class ZipkinTraceContent : HttpContent
    {
        private readonly IZipkinSpanWriter _writer;
        private readonly IEnumerable<Activity> _activities;

        public ZipkinTraceContent(IZipkinSpanWriter writer, IEnumerable<Activity> activities)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _activities = activities ?? throw new ArgumentNullException(nameof(activities));
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var writer = new Utf8JsonWriter(stream);
            _writer.Write(writer, _activities);
            return writer.FlushAsync();
        }

        protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            var writer = new Utf8JsonWriter(stream);
            _writer.Write(writer, _activities);
            writer.Flush();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}