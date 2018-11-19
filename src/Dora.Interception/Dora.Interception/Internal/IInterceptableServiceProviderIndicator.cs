namespace Dora.Interception.Internal
{
    internal interface IInterceptableServiceProviderIndicator
    {
        bool IsInterceptable { get; }
    }

    internal class InterceptableServiceProviderIndicator : IInterceptableServiceProviderIndicator
    {
        public bool IsInterceptable { get; }
        public InterceptableServiceProviderIndicator(bool isInterceptable)
        => IsInterceptable = isInterceptable;
    }
}
