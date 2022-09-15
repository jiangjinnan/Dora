using Dora.OpenTelemetry;
using Dora.OpenTelemetry.Tracing;
using System.Linq.Expressions;

namespace System.Diagnostics
{
    public static class ActivityExtensions
    {
        private static readonly Action<Activity?, ActivityKind> _kindSetter = CreateActivityKindSetter();

        public static IInstrumentation? GetInstrumentation(this Activity? activity)
        {
            return activity?.TagObjects?.SingleOrDefault(it => it.Key == "Instrumentation").Value as IInstrumentation;
        }

        public static Activity? SetInstrumentation(this Activity? activity, IInstrumentation instrumentation)
        {
            return activity?.SetTag("Instrumentation", instrumentation);
        }

        public static void RecordException(this Activity? activity, Exception ex)
        {
            activity?.RecordException(ex, default);
        }

        public static void RecordException(this Activity? activity, Exception ex, in TagList tags)
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

        public static Activity? ChangeKind(this Activity? activity, ActivityKind kind)
        {
            _kindSetter(activity, kind);
            return activity;
        }

        private static Action<Activity?, ActivityKind> CreateActivityKindSetter()
        {
            var instance = Expression.Parameter(typeof(Activity), "instance");
            var propertyValue = Expression.Parameter(typeof(ActivityKind), "propertyValue");
            var body = Expression.Assign(Expression.Property(instance, "Kind"), propertyValue);
            return Expression.Lambda<Action<Activity?, ActivityKind>>(body, instance, propertyValue).Compile();
        }
    }
}
