using Microsoft.Extensions.DependencyInjection;

namespace Dora.OpenTelemetry.Tracing
{
    public class TracingBuilder
    {
        public IServiceCollection Services { get; }
        public TracingBuilder(IServiceCollection services)=> Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
