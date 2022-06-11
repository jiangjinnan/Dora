using App5;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var method1 = MemberUtilities.GetMethod<Invoker>(it => it.Invoke());
var method2 = typeof(Invoker).GetMethod("Invoke");
var method3 = typeof(InvokerBase).GetMethod("Invoke");
var equal1 = method1 == method2;
var equal2 = method1 == method3;

var code1 = method1.MetadataToken;
var code2 = method2.MetadataToken;
var code3 = method3.MetadataToken;


var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .AddScoped<ScopedService>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(register: registry =>
        registry.For<FoobarInterceptor2>().ToMethod<Invoker>(order: 1, methodCall: it => it.Invoke())))
    .GetRequiredService<Invoker>();
await invoker.Invoke();

public class InvokerBase
{
    public virtual Task Invoke() => Task.Delay(1000);
}

public class Invoker: InvokerBase
{
    
}
