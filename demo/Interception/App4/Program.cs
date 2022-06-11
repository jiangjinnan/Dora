using App4;
using Microsoft.Extensions.DependencyInjection;

var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(register: registry =>
        {
            registry.For<FooInterceptor>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke1());
            registry.For<FooInterceptor>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke2());
            registry.For<FooInterceptor>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke3());

            registry.For<BarInterceptor>().ToMethod<Invoker>(order: 2, methodCall: it => it.Invoke1());
            registry.For<BarInterceptor>().ToMethod<Invoker>(order: 2, methodCall: it => it.Invoke2());
            registry.For<BarInterceptor>().ToMethod<Invoker>(order: 2, methodCall: it => it.Invoke3());
        }
        ))
    .GetRequiredService<Invoker>();
invoker.Invoke1();
Console.WriteLine();
await invoker.Invoke2();
Console.WriteLine();
await invoker.Invoke3();



public class Invoker
{
    public virtual void Invoke1() => Console.WriteLine("Invoker.Invoke1()");
    public virtual async Task Invoke2()
    { 
        await Task.Delay(1000);
        Console.WriteLine("Invoker.Invoke2()");
    }

    public virtual async ValueTask Invoke3()
    {
        await Task.Delay(1000);
        Console.WriteLine("Invoker.Invoke3()");
    }
}
