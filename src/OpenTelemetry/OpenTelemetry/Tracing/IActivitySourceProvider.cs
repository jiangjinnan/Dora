using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IActivitySourceProvider
    {
        ActivitySource GetActivitySource(string? name = null, string version = "");
    }
}