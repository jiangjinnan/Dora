using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    internal class ActivityListenerFactory : IActivityListenerFactory
    {
        private readonly TracingOptions _tracingOptions;
        private readonly IActivityProcessor _processor;
        private readonly ISampler _sampler;

        public ActivityListenerFactory(IOptions<TracingOptions> tracingOptions, IActivityProcessor processor, ISampler sampler)
        {
            _tracingOptions = (tracingOptions ?? throw new ArgumentNullException(nameof(tracingOptions))).Value;
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        }

        public ActivityListener CreateActivityListener()
        {
            return new ActivityListener
            {
                ActivityStarted = OnStarted,
                ActivityStopped = OnStopped,
                Sample = _sampler.Sample,
                ShouldListenTo = ShouldListenTo
            };
        }

        private bool ShouldListenTo(ActivitySource  activitySource)
        { 
            if (activitySource == null) throw new ArgumentNullException(nameof(activitySource));
            return _tracingOptions.ActivitySourceNames.Contains(activitySource.Name);
        }

        private void OnStarted(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            _processor.OnActivityStarted(activity);
        }

        private void OnStopped(Activity activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));
            if (!activity.IsAllDataRequested || SuppressScope.IsSuppressed)
            {
                return;
            }
            _processor.OnActivityStopped(activity);
        }
    }
}
