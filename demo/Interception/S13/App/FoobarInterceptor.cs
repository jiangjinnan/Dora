using Dora.Interception;
using System.Diagnostics;

namespace App
{
public class FoobarInterceptor
{
    public FoobarInterceptor(FoobarService foobarService)=> Debug.Assert(foobarService != null);
    public async  ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        Console.WriteLine($"[{GetType().Name}]: Before invoking");
        await invocationContext.ProceedAsync();
        Console.WriteLine($"[{GetType().Name}]: After invoking");
    }
}
}
