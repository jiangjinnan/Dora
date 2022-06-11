using App3;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

var calculator = new ServiceCollection()
    .AddSingleton<Calculator>()
    .BuildInterceptableServiceProvider()
    .GetRequiredService<Calculator>();

Console.WriteLine($"calculator.Add(1, 1) = {calculator.Add(1, 1)}");
