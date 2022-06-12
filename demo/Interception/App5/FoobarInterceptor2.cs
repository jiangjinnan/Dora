using Dora.Interception;

namespace App5
{
    internal class FoobarInterceptor2
    {
        public  ValueTask InvokeAsync(InvocationContext invocationContext,
            SingletonService singletonService1,
            SingletonService singletonService2,
            ScopedService scopedService1,
            ScopedService scopedService2,
            TransientService transientService1,
            TransientService transientService2)
            => invocationContext.ProceedAsync();
    }
}
