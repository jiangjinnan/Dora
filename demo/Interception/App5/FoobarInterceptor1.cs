using Dora.Interception;

namespace App5
{
    internal class FoobarInterceptor1
    {
        private readonly ScopedService _service;
        public FoobarInterceptor1(ScopedService service)=> _service = service;
        public  ValueTask InvokeAsync(InvocationContext invocationContext)=> invocationContext.ProceedAsync();
    }
}
