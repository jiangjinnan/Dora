using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;  
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clock1 = new ServiceCollection()
              .AddLogging(factory => factory.AddConsole())
              .AddMemoryCache()
              .AddSingleton<ISystomClock, SystomClock>()
              .AddInterception()
              .BuildServiceProvider()
              .GetRequiredService<IInterceptable<ISystomClock>>()
              .Proxy;

            var method = typeof(ISystomClock).GetMethod("GetCurrentTime");
            var field = clock1.GetType().GetField("_interceptors", BindingFlags.NonPublic | BindingFlags.Instance);
            var decoration = field.GetValue(clock1) as InterceptorDecoration;
            var interceptor = decoration.GetInterceptor(method);

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Current time: {clock1.GetCurrentTime(DateTimeKind.Local)}");
                Task.Delay(1000).Wait();
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Current time: {clock1.GetCurrentTime(DateTimeKind.Utc)}");
                Task.Delay(1000).Wait();
            }


            var clock2 = new ServiceCollection()
              .AddMemoryCache()
              .AddSingleton<ISystomClock, SystomClock>()
              .BuildInterceptableServiceProvider()
              .GetRequiredService<ISystomClock>();

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Current time: {clock2.GetCurrentTime(DateTimeKind.Local)}");
                Task.Delay(1000).Wait();
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Current time: {clock2.GetCurrentTime(DateTimeKind.Utc)}");
                Task.Delay(1000).Wait();
            }
        }
    }
}

