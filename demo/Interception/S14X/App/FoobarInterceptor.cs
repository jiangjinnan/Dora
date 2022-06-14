using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace App
{
public class FoobarInterceptor
{
    public async  ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        var provider = invocationContext.InvocationServices;

        _ = provider.GetRequiredService<SingletonService>();
        _ = provider.GetRequiredService<SingletonService>();

        _ = provider.GetRequiredService<ScopedService>();
        _ = provider.GetRequiredService<ScopedService>();

        _ = provider.GetRequiredService<TransientService>();
        _ = provider.GetRequiredService<TransientService>();

        Console.WriteLine($"[{GetType().Name}]: Before invoking");
        await invocationContext.ProceedAsync();
        Console.WriteLine($"[{GetType().Name}]: After invoking");
    }
}
}
