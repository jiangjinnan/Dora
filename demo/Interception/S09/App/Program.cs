using App;
using Microsoft.Extensions.DependencyInjection;

var calculator = new ServiceCollection()
    .AddSingleton<Calculator>()
    .BuildInterceptableServiceProvider()
    .GetRequiredService<Calculator>();

Console.WriteLine($"1 + 1 = {calculator.Add(1, 1)}");
