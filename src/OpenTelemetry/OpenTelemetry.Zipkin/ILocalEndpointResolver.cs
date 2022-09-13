namespace Dora.OpenTelemetry.Zipkin
{
    public interface ILocalEndpointResolver
    {
        ZipkinEndpoint Resolve(string serviceName);
    }
}
