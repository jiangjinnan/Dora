using App5;
using Microsoft.Extensions.DependencyInjection;

var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .AddScoped<ScopedService>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(register: registry =>
        registry.For<FoobarInterceptor1>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke())))
    .GetRequiredService<Invoker>();
await invoker.Invoke();



public class Invoker
{
    public virtual Task Invoke()=> Task.Delay(1000);
}
