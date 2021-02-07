namespace Dora.Interception
{
    public interface IInterceptor
    {
        bool CaptureArguments { get; }
        InterceptorDelegate Delegate { get; }
    }
}
