using Dora.Interception;

namespace App5
{
    internal class FoobarInterceptor2
    {
        public  ValueTask InvokeAsync(InvocationContext invocationContext, ScopedService service) => invocationContext.ProceedAsync();
    }
}
