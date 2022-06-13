using App;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<Foo>()
    .AddSingleton<Bar>()
    .BuildInterceptableServiceProvider();

var foo = provider.GetRequiredService<Foo>();
var bar = provider.GetRequiredService<Bar>();

foo.M();
foo.P1 = null;
_ = foo.P1;
foo.P2 = null;
_ = foo.P2;
Console.WriteLine();

bar.M();
bar.P = null;
_ = bar.P;