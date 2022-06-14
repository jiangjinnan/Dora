using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var foobar = new ServiceCollection()
    .AddSingleton<Foobar>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
    .GetRequiredService<Foobar>();

foobar.M();
foobar.P1 = null;
_ = foobar.P1;
foobar.P2 = null;
_ = foobar.P2;
Console.WriteLine("...");

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    registry.For<FoobarInterceptor>().ToAllMethods<Foobar>(order: 1);
    registry.SupressMethod<Foobar>(it=>it.M());
    registry.SupressProperty<Foobar>(it => it.P1);
    registry.SupressGetMethod<Foobar>(it => it.P2);
}