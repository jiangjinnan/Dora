using Dora.Interception;

namespace App
{
    public class InterceptorBase: InterceptorAttribute
    {
        public virtual async  ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine($"[{GetType().Name}]: Before invoking");
            await invocationContext.ProceedAsync();
            Console.WriteLine($"[{GetType().Name}]: After invoking");
        }
    }

    public class FooAttribute : InterceptorBase { }
    public class BarAttribute : InterceptorBase
    {
        public override ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine($"[{GetType().Name}]: Invoke");
            return ValueTask.CompletedTask;
        }
    }
    public class BazAttribute : InterceptorBase { }
}
