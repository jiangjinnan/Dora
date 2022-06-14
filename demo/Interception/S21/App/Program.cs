using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<Foobar>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors));

var foobar = provider.GetRequiredService<Foobar>();

foobar.M(1, 1);
foobar.M(3.14, 3.14);
foobar.P1 = null;
_ = foobar.P1;
foobar.P2 = null;
_ = foobar.P2;
foobar.P3 = null;
_ = foobar.P3;
Console.ReadLine();

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    var foobar = registry.For<FoobarInterceptor>();
    foobar
        .ToMethod<Foobar>(order: 1, it => it.M(default(int), default(int)))
        .ToMethod<Foobar>(order: 1, it => it.M(default(double), default(double)))
        .ToProperty<Foobar>(order: 1, it => it.P1)
        .ToGetMethod<Foobar>(order: 1, it => it.P2)
        .ToSetMethod<Foobar>(order: 1, it => it.P3)
        ;
}