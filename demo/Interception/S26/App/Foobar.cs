using Dora.Interception;

namespace App
{
    public interface IFoobar
    {
        Task InvokeAsync(int x, string y);
    }

    public class FoobarBase : IFoobar
    {
        //[FakeInterceptor]
        public virtual Task InvokeAsync(int x, string y) => Task.CompletedTask;
    }

    public class Foobar : FoobarBase
    {
        public override Task InvokeAsync(int x, string y)
        {
            return base.InvokeAsync(x, y);
        }
    }

    public class FakeInterceptorAttribute : InterceptorAttribute
    {
        public ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine("Fake...");
            return invocationContext.ProceedAsync();
        }

    }
}