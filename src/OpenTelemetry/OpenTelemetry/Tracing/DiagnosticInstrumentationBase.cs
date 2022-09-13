using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public abstract class DiagnosticInstrumentationBase : IDiagnosticInstrumentation
    {
        protected abstract string DiagnosticListenerName { get; }
        protected abstract string StartEventName { get; }
        protected abstract string StopEventName { get; }
        protected abstract string ErrorEventName { get; }
        public abstract string ActivitySourceName { get; }
        public bool Match(DiagnosticListener diagnosticListener) => diagnosticListener.Name == DiagnosticListenerName;
        public void OnNext(KeyValuePair<string, object?> payload)
        {
            var eventName = payload.Key;
            if (eventName == StartEventName)
            {               
                OnStarted(Activity.Current, payload.Value);
            }
            else if (eventName == StopEventName)
            {
                OnStopped(Activity.Current, payload.Value);
            }
            else if (eventName == ErrorEventName)
            {
                OnError(Activity.Current, payload.Value);
            }
        }

        protected virtual void OnStarted(Activity? activity, object? payload) { }
        protected virtual void OnStopped(Activity? activity, object? payload)
        {
            if (activity == null)
            {
                return;
            }
            if (activity.GetInstrumentation() == this)
            {
                if (activity.IsAllDataRequested)
                { 
                    activity.SetStatus(ActivityStatusCode.Ok);
                }
                activity?.Stop();
            }
        }
        protected virtual void OnError(Activity? activity, object? payload)
        {
            if (activity == null)
            {
                return;
            }
            if (activity.GetInstrumentation() == this)
            {
                if (activity.IsAllDataRequested)
                {
                    activity.SetStatus(ActivityStatusCode.Error);
                }
                activity?.Stop();
            }
        }
        protected T GetpayloadMember<T>(object? payload, string propertyName)
        {
            if (payload is null)
            {
                return default!;
            }
            return PropertyExtractor.GetValue<T>(payload, propertyName);
        }
    }
}
