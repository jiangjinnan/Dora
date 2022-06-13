using Dora.Interception;

namespace App
{
    public class FoobarInterceptor
    {
        public ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            invocationContext.SetArgument("x", 0);
            invocationContext.SetArgument("y", 0);
            return invocationContext.ProceedAsync();
        }
    }
}
