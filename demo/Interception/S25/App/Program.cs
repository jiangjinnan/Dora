using App;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<Foobar>()
    .BuildInterceptableServiceProvider(interception => interception.RegisterInterceptors(RegisterInterceptors));

var foobar = provider.GetRequiredService<Foobar>();

foobar.M();
_ = foobar.P;
foobar.P = null;
Console.ReadLine();

static void RegisterInterceptors(ConditionalInterceptorProviderOptions options)
{
    options.For<FoobarInterceptor>()        
        .To(1, (type, method) => type == typeof(Foobar) && method.Name == "M")
        .To(1, (type, method) => type == typeof(Foobar) && method.IsSpecialName && method.Name == "set_P");
}