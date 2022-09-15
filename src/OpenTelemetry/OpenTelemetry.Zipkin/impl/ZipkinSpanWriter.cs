using Dora.OpenTelemetry.Tracing;
using System.Diagnostics;
using System.Text.Json;

namespace Dora.OpenTelemetry.Zipkin
{
    internal class ZipkinSpanWriter : IZipkinSpanWriter
    {
        private readonly ActivityTagsCollection _resourceAttributes;
        private readonly ZipkinEndpoint _localEndpoint;

        public ZipkinSpanWriter(IResourceProvider  resourceProvider, ILocalEndpointResolver localEndpointResolver)
        {
            _resourceAttributes = (resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider))).GetAttributes();
            var serviceNameEntry = _resourceAttributes.SingleOrDefault(it => it.Key == OpenTelemetryDefaults.ResourceAttributes.ServiceName);
            if (serviceNameEntry.Key is null)
            {
                throw new InvalidOperationException("Service name is not configured.");
            }
            _localEndpoint = (localEndpointResolver ?? throw new ArgumentNullException(nameof(localEndpointResolver))).Resolve(serviceNameEntry.Value!.ToString()!);
        }

        public void Write(Utf8JsonWriter writer, IEnumerable<Activity> activities)
        {
            writer.WriteStartArray();
            foreach (var activity in activities)
            {
                WriteActivity(writer, activity);
            }
            writer.WriteEndArray();
        }

        private void WriteActivity(Utf8JsonWriter writer, Activity activity)
        {
            writer.WriteStartObject();
            WriteLocalEndpoint(writer);
            writer.WriteString(ZipkinDefaults.SpanPropertyNames.TraceId, activity.TraceId.ToHexString());
            writer.WriteString(ZipkinDefaults.SpanPropertyNames.Id, activity.SpanId.ToHexString());
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.Name, activity.DisplayName);
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.ParentId, activity.ParentSpanId.ToHexString());
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.Kind, GetKind(activity));
            writer.WriteNumberIfExists(ZipkinDefaults.SpanPropertyNames.Timestamp, activity.StartTimeUtc.AsEpochMicroseconds());
            writer.WriteNumberIfExists(ZipkinDefaults.SpanPropertyNames.Duration, activity.Duration.AsEpochMicroseconds());

            WriteEvents(writer, activity);
            WriteTags(writer, activity);
            writer.WriteEndObject();
        }

        private static void WriteEvents(Utf8JsonWriter writer, Activity activity)
        {
            var events = activity.Events;
            if (!events.Any())
            {
                return;
            }

            writer.WritePropertyName(ZipkinDefaults.SpanPropertyNames.Annotations);
            writer.WriteStartArray();
            foreach (var @event in events)
            {
                writer.WriteStartObject();
                writer.WriteNumber(ZipkinDefaults.SpanPropertyNames.Timestamp, @event.Timestamp.DateTime.AsEpochMicroseconds());
                writer.WriteString(ZipkinDefaults.SpanPropertyNames.Value, @event.Name);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        private void WriteTags(Utf8JsonWriter writer, Activity activity)
        {
            string? peerService = null;
            string? peerHostName = null;
            string? peerAddress = null;
            string? httpHost = null;
            string? dbInstance = null;

            writer.WritePropertyName(ZipkinDefaults.SpanPropertyNames.Tags);
            writer.WriteStartObject();
            foreach (var attribute in _resourceAttributes)
            {
                if (ZipkinTagTransformer.Instance.TryTransformTag(attribute, out var result))
                {
                    writer.WriteStringIfExists(attribute.Key, result);
                }
            }
           
            var isStatusAdded = false;
            if (activity.Status != ActivityStatusCode.Unset)
            {
                if (activity.Status == ActivityStatusCode.Ok)
                {
                    writer.WriteString(OpenTelemetryDefaults.SpanAttributeNames.StatusCode, "OK");
                }
                else
                {
                    writer.WriteString(OpenTelemetryDefaults.SpanAttributeNames.StatusCode, "ERROR");
                    writer.WriteStringIfExists(OpenTelemetryDefaults.SpanAttributeNames.StatusDescription, activity.StatusDescription);
                }
                isStatusAdded = true;
            }
           
            foreach (var tagObject in activity.TagObjects)
            {
                if (ResolveRemotePeer(tagObject.Key, tagObject.Value))
                {
                    continue;
                }
                if (isStatusAdded && tagObject.Key == OpenTelemetryDefaults.SpanAttributeNames.StatusCode)
                {
                    continue;
                }
                if (ZipkinTagTransformer.Instance.TryTransformTag(tagObject, out var result))
                {
                    writer.WriteString(tagObject.Key, result);
                }
            }
            if (activity.Kind == ActivityKind.Client || activity.Kind == ActivityKind.Producer)
            {
                writer.WriteStringIfExists(OpenTelemetryDefaults.SpanAttributeNames.PeerService, peerService ?? peerHostName ?? peerAddress ?? httpHost ?? dbInstance);
            }
            writer.WriteEndObject();

            bool ResolveRemotePeer(string key, object? value)
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

        private void WriteLocalEndpoint(Utf8JsonWriter writer)
        {
            writer.WritePropertyName(ZipkinDefaults.SpanPropertyNames.LocalEndpoint);
            writer.WriteStartObject();
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.ServiceName, _localEndpoint.ServiceName);
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.Ipv4, _localEndpoint.Ipv4);
            writer.WriteStringIfExists(ZipkinDefaults.SpanPropertyNames.Ipv6, _localEndpoint.Ipv6);
            writer.WriteNumberIfExists(ZipkinDefaults.SpanPropertyNames.Port, _localEndpoint.Port);
            writer.WriteEndObject();
        }
        private static string? GetKind(Activity activity)
        {
            return activity.Kind switch
            {
                ActivityKind.Server => "SERVER",
                ActivityKind.Producer => "PRODUCER",
                ActivityKind.Consumer => "CONSUMER",
                ActivityKind.Client => "CLIENT",
                _ => null,
            };
        }
    }
}
