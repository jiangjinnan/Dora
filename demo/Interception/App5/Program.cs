using App5;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<Invoker>()
    .AddSingleton<SingletonService>()
    .AddScoped<ScopedService>()
    .AddTransient<TransientService>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(register: registry =>
        registry.For<FoobarInterceptor2>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke())));
using (provider as IDisposable)
{ 
    var invoker = provider.GetRequiredService<Invoker>();
    await invoker.Invoke();
    Console.WriteLine();
    await invoker.Invoke();
}

public class InvokerBase
{
    [NonInterceptable]
    public virtual Task Invoke() => Task.Delay(1000);
}

public class Invoker: InvokerBase
{
    
}
