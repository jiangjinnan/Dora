namespace Dora.OpenTelemetry.OpenTelemetryProtocol
{
    public interface IPayloadDeliverer<TPayload>
    {
        void Send(TPayload payload);
    }
}
