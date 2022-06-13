using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .AddSingleton<IFoobar, Foo>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
    .GetRequiredService<Invoker>();

invoker.M1();
Console.WriteLine();
invoker.M2();

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    registry.For<FoobarInterceptor>("Interceptor1").ToMethod<Invoker>(order: 1, it => it.M1());
    registry.For<FoobarInterceptor>("Interceptor2", new Bar()).ToMethod<Invoker>(order: 1, it => it.M2());
}