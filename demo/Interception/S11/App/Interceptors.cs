using Dora.Interception;

namespace App
{
    public class InterceptorBase
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine($"[{GetType().Name}]: Before invoking");
            await invocationContext.ProceedAsync();
            Console.WriteLine($"[{GetType().Name}]: After invoking");
        }
    }

    public class FooInterceptor : InterceptorBase { }
    public class BarInterceptor : InterceptorBase { }
    public class BazInterceptor : InterceptorBase { }
}
