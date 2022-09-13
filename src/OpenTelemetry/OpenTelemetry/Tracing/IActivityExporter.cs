using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IActivityExporter
    {
        void Export(IEnumerable<Activity> activities);
    }
}
