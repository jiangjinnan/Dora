using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<Foobar>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors));

var foobar = provider.GetRequiredService<Foobar>();

foobar.M();
foobar.P1 = null;
_ = foobar.P1;
foobar.P2 = null;
_ = foobar.P2;
Console.WriteLine();

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    var foobar = registry.For<FoobarInterceptor>();
    foobar.ToAllMethods<Foobar>(order: 1);
}