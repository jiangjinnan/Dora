using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public abstract class SamplerBase : ISampler
    {
        public ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options)
        {
            if (SuppressScope.IsSuppressed) return ActivitySamplingResult.None;
            return SampleCore(ref options);
        }

        protected abstract ActivitySamplingResult SampleCore(ref ActivityCreationOptions<ActivityContext> options);
    }
}
