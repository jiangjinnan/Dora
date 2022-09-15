namespace Dora.OpenTelemetry.Tracing
{
    public class TracingOptions
    {
        public ISet<string> ActivitySourceNames { get; } = new HashSet<string> { "" };
        public BatchingOptions Batching { get; set; } = new BatchingOptions();
        public ServiceInstance ServiceInstance { get; set; } = default!;
    }

    public class BatchingOptions
    {
        public int BufferCapacity { get; set; } = 500;
        public int BatchSize { get; set; } = 50;
        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan DeliveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    }

    public class ServiceInstance
    {
        public ServiceInstance(string name, string? @namespace, string? version, string? instanceId)
        {
            Name = Guard.ArgumentNotNullOrWhitespace(name);
            Namespace = @namespace;
            Version = version;
            InstanceId = instanceId;
        }

        public string Name { get; }
        public string? Namespace { get; }
        public string? Version { get; }
        public string? InstanceId { get; }
    }
}
