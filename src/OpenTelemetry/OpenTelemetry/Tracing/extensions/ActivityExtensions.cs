using Dora.OpenTelemetry;
using Dora.OpenTelemetry.Tracing;

namespace System.Diagnostics
{
    public static class ActivityExtensions
    {
        public static IInstrumentation? GetInstrumentation(this Activity? activity)
        {
            return activity?.TagObjects?.SingleOrDefault(it => it.Key == "Instrumentation").Value as IInstrumentation;
        }

        public static Activity? SetInstrumentation(this Activity? activity, IInstrumentation instrumentation)
        {
            return activity?.SetTag("Instrumentation", instrumentation);
        }

        public static void RecordException(this Activity activity, Exception ex)
        {
            activity?.RecordException(ex, default);
        }

        public static void RecordException(this Activity activity, Exception ex, in TagList tags)
        {
            if (ex == null || activity == null)
            {
                return;
            }

            var tagsCollection = new ActivityTagsCollection
            {
                { OpenTelemetryDefaults.SpanAttributeNames.ExceptionType, ex.GetType().FullName },
                { OpenTelemetryDefaults.SpanAttributeNames.ExceptionStacktrace, ex.StackTrace },
            };

            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                tagsCollection.Add(OpenTelemetryDefaults.SpanAttributeNames.ExceptionMessage, ex.Message);
            }

            foreach (var tag in tags)
            {
                tagsCollection[tag.Key] = tag.Value;
            }

            activity.AddEvent(new ActivityEvent(OpenTelemetryDefaults.SpanAttributeNames.ExceptionEventName, default, tagsCollection));
        }
    }
}
