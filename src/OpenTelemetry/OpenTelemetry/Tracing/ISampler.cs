using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface ISampler
    {
        ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options);
    }
}
