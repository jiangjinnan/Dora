using App;
using Dora.Interception;
using Dora.Interception.CodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var foobar = new ServiceCollection()
    .AddSingleton<FoobarBase, Foobar>()
    .AddLogging(logging=>logging.AddConsole())
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
    .GetRequiredService<FoobarBase>();

await foobar.InvokeAsync(default, default);

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    registry.For<FakeInterceptorAttribute>().ToMethod<Foobar>(1, it => it.InvokeAsync(default,default));
}