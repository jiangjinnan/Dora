using System;

namespace Dora.Interception
{
    public class Interceptor : IInterceptor
    {
        public InterceptorDelegate Delegate { get; }
        public bool CaptureArguments { get; }

        public Interceptor(InterceptorDelegate @delegate, bool captureArguments)
        {
            Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            CaptureArguments = captureArguments;
        }
    }
}
