using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    internal class AlwaysOnSampler : SamplerBase
    {
        protected override ActivitySamplingResult SampleCore(ref ActivityCreationOptions<ActivityContext> options)
        {
          

            if (options.Parent == default)
            {
                return ActivitySamplingResult.AllDataAndRecorded;
            }
            return options.Parent.TraceFlags == ActivityTraceFlags.Recorded ? ActivitySamplingResult.AllDataAndRecorded : ActivitySamplingResult.AllData;
        }
    }
}
