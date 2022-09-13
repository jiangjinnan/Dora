using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IActivityProcessor
    {
        void OnActivityStopped(Activity activity);
        void OnActivityStarted(Activity activity);
    }
}
