using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var foobar = new ServiceCollection()
    .AddSingleton<Foobar>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors))
    .GetRequiredService<Foobar>();

foobar.M();
foobar.P = null;
_ = foobar.P;

static void RegisterInterceptors(IInterceptorRegistry registry)
{
    var foobar = registry.For<FoobarInterceptor>();
    foobar.ToAllMethods<Foobar>(order: 1);
}