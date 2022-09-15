using Opentelemetry.Proto.Common.V1;

namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{

    internal sealed class OtlpTagTransformer : TagTransformer<AnyValue>
    {
        private OtlpTagTransformer() { }
        public static OtlpTagTransformer Instance { get; } = new();
        protected override AnyValue TransformInt64(string key, long value) => new() { IntValue = value };
        protected override AnyValue TransformDouble(string key, double value) => new() { DoubleValue = value };
        protected override AnyValue TransformBoolean(string key, bool value) => new() { BoolValue = value };
        protected override AnyValue TransformString(string key, string value) => new() { StringValue = value };
        protected override AnyValue TransformArray(string key, Array array)
        {
            var arrayValue = new ArrayValue();
            foreach (var item in array)
            {
                try
                {
                    var value = item != null
                        ? TryTransformTag(new KeyValuePair<string, object?>("", item), out var result) ? result : new AnyValue()
                        : new AnyValue();
                    arrayValue.Values.Add(value);
                }
                catch
                {
                    return null!;
                }
            }

            return new AnyValue { ArrayValue = arrayValue };
        }
    }
}
