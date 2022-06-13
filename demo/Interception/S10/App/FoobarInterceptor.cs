using Dora.Interception;

namespace App
{
    public class FoobarInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            await invocationContext.ProceedAsync();
            invocationContext.SetReturnValue(0);
        }
    }
}
