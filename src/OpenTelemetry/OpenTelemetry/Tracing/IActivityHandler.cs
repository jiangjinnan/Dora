using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IActivityHandler
    {
        int Order { get; }
        void OnActivityStarted(Activity activity);
        void OnActivityStopped(Activity activity);
    }
}
