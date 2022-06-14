using Dora.Interception;
using System.Diagnostics;

namespace App
{
public class FoobarInterceptor
{
    public async  ValueTask InvokeAsync(InvocationContext invocationContext,
        SingletonService singletonService1, SingletonService singletonService2,
        ScopedService scopedService1, ScopedService scopedService2,
        TransientService transientService1, TransientService transientService2)
    {
        Console.WriteLine($"[{GetType().Name}]: Before invoking");
        await invocationContext.ProceedAsync();
        Console.WriteLine($"[{GetType().Name}]: After invoking");
    }
}
}
