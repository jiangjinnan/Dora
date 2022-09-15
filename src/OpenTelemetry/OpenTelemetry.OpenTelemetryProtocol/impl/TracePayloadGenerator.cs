using Dora.OpenTelemetry.Tracing;
using Google.Protobuf;
using Opentelemetry.Proto.Collector.Trace.V1;
using Opentelemetry.Proto.Common.V1;
using Opentelemetry.Proto.Resource.V1;
using Opentelemetry.Proto.Trace.V1;
using System.Diagnostics;
using static Opentelemetry.Proto.Trace.V1.Status.Types;

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    internal class TracePayloadGenerator : IPayloadGenerator<ExportTraceServiceRequest>
    {
        private readonly Resource _resource;

        public TracePayloadGenerator(IResourceProvider resourceProvider)
        {
            var attributes = (resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider))).GetAttributes();
            _resource = new Resource();
            foreach (var kv in attributes)
            {
                if (OtlpTagTransformer.Instance.TryTransformTag(kv, out var result))
                {
                    _resource.Attributes.Add(new KeyValue { Key = kv.Key, Value = result });
                }
            }
        }

        public ExportTraceServiceRequest Generate(IEnumerable<Activity> activities)
        {
            var request = new ExportTraceServiceRequest();

            var spansByLibrary = new Dictionary<string, ScopeSpans>();
            var  resourceSpans = new ResourceSpans
            {
                Resource = _resource,
            };
            request.ResourceSpans.Add(resourceSpans);

            foreach (var activity in activities)
            {
                var span = AsOtlpSpan(activity);
                if (span is not null)
                {
                    var activitySourceName = activity.Source.Name;
                    if (!spansByLibrary.TryGetValue(activitySourceName, out var spans))
                    {
                        spans = new ScopeSpans
                        {
                            Scope = new InstrumentationScope
                            {
                                Name = activitySourceName, 
                                Version = activity.Source.Version ?? string.Empty
                            },
                        };
                        spansByLibrary.Add(activitySourceName, spans);
                        resourceSpans.ScopeSpans.Add(spans);
                    }

                    spans.Spans.Add(span);
                }               
            }
            return request;
        }

        private static Span? AsOtlpSpan(Activity activity)
        {
            if (activity.IdFormat != ActivityIdFormat.W3C)
            {
                return null;
            }

            var span = CreateOtlpSpan(activity);
            var status = ExtractTags(activity, span);
            SetStatus(activity, span, status);
            ExtractEvents(activity, span);
            ExtractLinks(activity);
            return span;

            static Span CreateOtlpSpan(Activity activity)
            {
                var traceIdBytes = new byte[16];
                var spanIdBytes = new byte[8];
                activity.TraceId.CopyTo(traceIdBytes);
                activity.SpanId.CopyTo(spanIdBytes);

                var parentSpanIdString = ByteString.Empty;
                if (activity.ParentSpanId != default)
                {
                    byte[] parentSpanIdBytes = new byte[8];
                    activity.ParentSpanId.CopyTo(parentSpanIdBytes);
                    parentSpanIdString = UnsafeByteOperations.UnsafeWrap(parentSpanIdBytes);
                }

                var startTimeUnixNano = activity.StartTimeUtc.ToUnixTimeNanoseconds();
                var span = new Span
                {
                    Name = activity.DisplayName,
                    Kind = (Span.Types.SpanKind)(activity.Kind + 1),
                    TraceId = UnsafeByteOperations.UnsafeWrap(traceIdBytes),
                    SpanId = UnsafeByteOperations.UnsafeWrap(spanIdBytes),
                    ParentSpanId = parentSpanIdString,
                    StartTimeUnixNano = (ulong)startTimeUnixNano,
                    EndTimeUnixNano = (ulong)(startTimeUnixNano + activity.Duration.ToNanoseconds()),
                };
                return span;
            }
            static void SetStatus(Activity activity, Span span, ActivityStatusCode status)
            {
                var statusCode = status switch
                {
                    ActivityStatusCode.Error => StatusCode.Error,
                    ActivityStatusCode.Ok => StatusCode.Ok,
                    _ => StatusCode.Unset
                };
                span.Status = new Status { Code = statusCode };
                var statusDescription = activity.StatusDescription;
                if (!string.IsNullOrWhiteSpace(statusDescription))
                {
                    span.Status.Message = statusDescription;
                }
            }
            static void ExtractEvents(Activity activity, Span span)
            {
                foreach (var @event in activity.Events)
                {
                    var otlpEvent = new Span.Types.Event
                    {
                        Name = @event.Name,
                        TimeUnixNano = (ulong)@event.Timestamp.ToUnixTimeNanoseconds(),
                    };
                    foreach (var kv in @event.Tags)
                    {
                        if (OtlpTagTransformer.Instance.TryTransformTag(kv, out var result))
                        {
                            span.Attributes.Add(new KeyValue { Key = kv.Key, Value = result });
                        }
                    }
                    span.Events.Add(otlpEvent);
                }
            }
            static void ExtractLinks(Activity activity)
            {
                foreach (var link in activity.Links)
                {
                    var linkTraceIdBytes = new byte[16];
                    var linkSpanIdBytes = new byte[8];
                    link.Context.TraceId.CopyTo(linkTraceIdBytes);
                    link.Context.SpanId.CopyTo(linkSpanIdBytes);
                    var otlpLink = new Span.Types.Link
                    {
                        TraceId = UnsafeByteOperations.UnsafeWrap(linkTraceIdBytes),
                        SpanId = UnsafeByteOperations.UnsafeWrap(linkSpanIdBytes),
                        TraceState = link.Context.TraceState
                    };

                    if (link.Tags is not null)
                    {
                        foreach (var kv in link.Tags)
                        {
                            if (OtlpTagTransformer.Instance.TryTransformTag(kv, out var result))
                            {
                                otlpLink.Attributes.Add(new KeyValue { Key = kv.Key, Value = result });
                            }
                        }
                    }
                }
            }
        }

        private static ActivityStatusCode ExtractTags(Activity activity, Span span)
        {
            ActivityStatusCode status = activity.Status;
            string? peerService = null;
            string? peerHostName = null;
            string? peerAddress = null;
            string? httpHost = null;
            string? dbInstance = null;

            foreach (var tagObject in activity.TagObjects)
            {
                if (TryResolveRemotePeer(tagObject.Key, tagObject.Value))
                {
                    continue;
                }
                if (status != ActivityStatusCode.Unset && tagObject.Key == OpenTelemetryDefaults.SpanAttributeNames.StatusCode)
                {
                    status = tagObject.Value?.ToString() == "Error" ? ActivityStatusCode.Error : ActivityStatusCode.Ok;
                    continue;
                }
                if (OtlpTagTransformer.Instance.TryTransformTag(tagObject, out var result))
                {
                    span.Attributes.Add(new KeyValue { Key = tagObject.Key, Value = result });
                }
            }
            if (activity.Kind == ActivityKind.Client || activity.Kind == ActivityKind.Producer)
            {
                var peerServiceName = peerService ?? peerHostName ?? peerAddress ?? httpHost ?? dbInstance;
                if (!string.IsNullOrWhiteSpace(peerServiceName))
                {
                    var kv = new KeyValue { Key = OpenTelemetryDefaults.SpanAttributeNames.PeerService, Value = new AnyValue { StringValue = peerServiceName } };
                    span.Attributes.Add(kv);
                }
            }

            return status;

            bool TryResolveRemotePeer(string key, object? value)
            {
                if (activity.Kind == ActivityKind.Client || activity.Kind == ActivityKind.Producer)
                {
                    if (key == OpenTelemetryDefaults.SpanAttributeNames.PeerService) { peerAddress = value?.ToString(); return true; }
                    if (key == OpenTelemetryDefaults.SpanAttributeNames.PeerHostName) { peerHostName = value?.ToString(); return true; }
                    if (key == OpenTelemetryDefaults.SpanAttributeNames.PeerAddress) { peerAddress = value?.ToString(); return true; }
                    if (key == OpenTelemetryDefaults.SpanAttributeNames.HttpHost) { httpHost = value?.ToString(); return true; }
                    if (key == OpenTelemetryDefaults.SpanAttributeNames.DbInstance) { dbInstance = value?.ToString(); return true; }
                }
                return false;
            }
        }
    }
}
