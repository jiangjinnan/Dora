using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clock1 = new ServiceCollection()
              .AddMemoryCache()
              .AddSingleton<ISystomClock, SystomClock>()
              .AddInterception(builder => builder.SetDynamicProxyFactory())
              .BuildServiceProvider()
              .GetRequiredService<IInterceptable<ISystomClock>>()
              .Proxy;
            for (int i = 0; i < int.MaxValue; i++)
            {
                Console.WriteLine($"Current time: {clock1.GetCurrentTime()}");
                Task.Delay(1000).Wait();
            }


            var clock2 = new ServiceCollection()
              .AddMemoryCache()
              .AddSingleton<ISystomClock, SystomClock>()
              .BuilderInterceptableServiceProvider()
              .GetRequiredService<ISystomClock>();

            for (int i = 0; i < int.MaxValue; i++)
            {
                Console.WriteLine($"Current time: {clock2.GetCurrentTime()}");
                Task.Delay(1000).Wait();
            }
        }
    }
}
