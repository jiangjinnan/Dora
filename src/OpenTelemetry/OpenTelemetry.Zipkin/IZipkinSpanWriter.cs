using System.Diagnostics;
using System.Text.Json;

namespace Dora.OpenTelemetry.Zipkin
{
    public interface IZipkinSpanWriter
    {
        void Write(Utf8JsonWriter writer, IEnumerable<Activity> activities);
    }
}
