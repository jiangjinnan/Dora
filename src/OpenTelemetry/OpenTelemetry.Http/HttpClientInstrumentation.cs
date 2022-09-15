using Dora.OpenTelemetry.Tracing;
using System.Diagnostics;
using System.Net.Sockets;

namespace Dora.OpenTelemetry.Http
{
    public class HttpClientInstrumentation : DiagnosticInstrumentationBase
    {
        public override string ActivitySourceName => "";

        protected override string DiagnosticListenerName => "HttpHandlerDiagnosticListener";

        protected override string StartEventName => "System.Net.Http.HttpRequestOut.Start";

        protected override string StopEventName => "System.Net.Http.HttpRequestOut.Stop";

        protected override string ErrorEventName => "System.Net.Http.Exception";

        protected override void OnStarted(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested == true && !SuppressScope.IsSuppressed)
            {
                var request = GetpayloadMember<HttpRequestMessage>(payload, "Request");
                if (request == null)
                {
                    return;
                }

                activity.DisplayName = $"HTTP {request.Method}";
                activity.ChangeKind(ActivityKind.Client);

                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpMethod, request.Method.ToString());
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpHost, request.RequestUri?.Host);
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpUrl, request.RequestUri?.ToString());
            }
        }

        protected override void OnStopped(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested == true)
            {
                var requestTaskStatus = GetpayloadMember<TaskStatus>(payload, "RequestTaskStatus");
                //Faulted=>OnError
                if (requestTaskStatus != TaskStatus.RanToCompletion && requestTaskStatus != TaskStatus.Faulted && activity.Status == ActivityStatusCode.Unset)
                {
                    activity.SetStatus(ActivityStatusCode.Error);
                }

                var response = GetpayloadMember<HttpResponseMessage>(payload, "Response");
                
                if (response is not null)
                {
                    var responseStatusCode = (int)response.StatusCode;
                    activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpStatusCode, responseStatusCode);
                    if (activity.Status == ActivityStatusCode.Unset && responseStatusCode>=400 && responseStatusCode<=499)
                    {
                        activity.SetStatus(ActivityStatusCode.Error);
                    }
                }               
            }
        }

        protected override void OnError(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested == true)
            {
                var exception = GetpayloadMember<Exception>(payload, "exception");
                if (exception is not null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, exception.Message);
                    activity.RecordException(exception);
                }
            }
        }
    }
}