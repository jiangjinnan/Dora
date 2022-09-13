using Dora.OpenTelemetry.Tracing;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Dora.OpenTelemetry.AspNetCore
{
    public class AspNetCoreInstrumentation : DiagnosticInstrumentationBase
    {
        public override string ActivitySourceName => "Microsoft.AspNetCore";

        protected override string DiagnosticListenerName => "Microsoft.AspNetCore";

        protected override string StartEventName => "Microsoft.AspNetCore.Hosting.BeginRequest";

        protected override string StopEventName => "Microsoft.AspNetCore.Hosting.EndRequest";

        protected override string ErrorEventName => "Microsoft.AspNetCore.Diagnostics.UnhandledException";

        protected override void OnStarted(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested == true)
            {
                var httpContext = GetpayloadMember<HttpContext>(payload, "httpContext");
                if (httpContext is null)
                {
                    return;
                }
                var request = httpContext.Request;
                var path = (request.PathBase.HasValue || request.Path.HasValue) ? (request.PathBase + request.Path).ToString() : "/";
                activity.DisplayName = path;

                if (request.Host.Port is null or 80 or 443)
                {
                    activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpHost, request.Host.Host);
                }
                else
                {
                    activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpHost, request.Host.Host + ":" + request.Host.Port);
                }

                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpMethod, request.Method);
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpScheme, request.Scheme);
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpTarget, path);
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpUrl, GetUri(request));
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpFlavor, GetFlavorTagValueFromProtocol(request.Protocol));

                var userAgent = request.Headers["User-Agent"].FirstOrDefault();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpUserAgent, userAgent);
                }
            }
        }

        protected override void OnStopped(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested == true)
            {
                var httpContext = GetpayloadMember<HttpContext>(payload, "httpContext");
                if (httpContext is null)
                {
                    return;
                }
                var response = httpContext.Response;
                activity.SetTag(OpenTelemetryDefaults.SpanAttributeNames.HttpStatusCode, response.StatusCode);
                if (activity.Status == ActivityStatusCode.Unset)
                {
                    activity.SetStatus(ResolveSpanStatusForHttpStatusCode(activity.Kind, response.StatusCode));
                }
            }
        }

        protected override void OnError(Activity? activity, object? payload)
        {
            if (activity?.IsAllDataRequested != true)
            {
                return;
            }

            var excepiton = GetpayloadMember<Exception>(payload, "exception");
            if (excepiton is not null)
            {
                activity.RecordException(excepiton);
                activity.SetStatus(ActivityStatusCode.Error, excepiton.Message);
            }
        }

        public static string GetFlavorTagValueFromProtocol(string protocol)
        {
            return protocol switch
            {
                "HTTP/2" => "2.0",
                "HTTP/3" => "3.0",
                "HTTP/1.1" => "1.1",
                _ => protocol,
            };
        }

        private static string GetUri(HttpRequest request)
        {           
            var scheme = request.Scheme ?? string.Empty;
            var host = request.Host.Value ?? "UNKNOWN-HOST";
            var pathBase = request.PathBase.Value ?? string.Empty;
            var path = request.Path.Value ?? string.Empty;
            var queryString = request.QueryString.Value ?? string.Empty;
            var length = scheme.Length + Uri.SchemeDelimiter.Length + host.Length + pathBase.Length
                         + path.Length + queryString.Length;

            return string.Create(length, (scheme, host, pathBase, path, queryString), (span, parts) =>
            {
                CopyTo(ref span, parts.scheme);
                CopyTo(ref span, Uri.SchemeDelimiter);
                CopyTo(ref span, parts.host);
                CopyTo(ref span, parts.pathBase);
                CopyTo(ref span, parts.path);
                CopyTo(ref span, parts.queryString);

                static void CopyTo(ref Span<char> buffer, ReadOnlySpan<char> text)
                {
                    if (!text.IsEmpty)
                    {
                        text.CopyTo(buffer);
                        buffer = buffer.Slice(text.Length);
                    }
                }
            });
        }

        private static ActivityStatusCode ResolveSpanStatusForHttpStatusCode(ActivityKind kind, int httpStatusCode)
        {
            var upperBound = kind == ActivityKind.Client ? 399 : 499;
            if (httpStatusCode >= 100 && httpStatusCode <= upperBound)
            {
                return ActivityStatusCode.Unset;
            }

            return ActivityStatusCode.Error;
        }
    }
}