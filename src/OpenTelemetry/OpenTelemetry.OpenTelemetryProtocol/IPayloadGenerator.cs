using System.Diagnostics;

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    public interface IPayloadGenerator<TPayload>
    {
        TPayload Generate(IEnumerable<Activity> activities);
    }
}
