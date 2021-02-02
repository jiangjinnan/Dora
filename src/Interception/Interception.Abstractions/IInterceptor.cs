namespace Dora.Interception
{
    public interface IInterceptor
    {
        bool AlterArguments { get; }
        bool CaptureArguments { get; }
        InterceptorDelegate Delegate { get; }
    }
}
