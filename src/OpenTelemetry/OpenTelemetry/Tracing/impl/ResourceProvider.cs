using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    internal class ResourceProvider : IResourceProvider
    {
        private ActivityTagsCollection? _attributes;
        private readonly TracingOptions _options;

        public ResourceProvider(IOptions<TracingOptions> optionsAccessor)
        {
            _options = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value;
        }
        public ActivityTagsCollection GetAttributes()
        {
            if (_attributes == null)
            {
                _attributes = new ActivityTagsCollection();
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.SdkName, "dora");
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.SdkVersion, "1.0.0");
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.SdkLanguage, "dotnet");

                var serviceInstance = _options.ServiceInstance;
                if (serviceInstance is null) throw new InvalidOperationException("Service instance based resource attributes are not configured.");

                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.ServiceName, serviceInstance.Name);
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.ServiceNamespace, serviceInstance.Namespace);
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.ServiceVersion, serviceInstance.Version);
                _attributes.Add(OpenTelemetryDefaults.ResourceAttributes.ServiceInstanceId, serviceInstance.InstanceId);
            }
            return _attributes;
        }
    }
}
