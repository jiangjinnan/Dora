using System.Diagnostics;

namespace Dora.OpenTelemetry
{
    public interface IActivityListenerFactory
    {
        ActivityListener CreateActivityListener();
    }
}
