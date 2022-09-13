using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IResourceProvider
    {
        ActivityTagsCollection GetAttributes();
    }
}
