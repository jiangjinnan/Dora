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
        static void Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddSingleton<Demo, Demo>()
                .BuilderInterceptableServiceProvider();

            var demo = provider.GetRequiredService<Demo>();
            demo = provider.GetRequiredService<Demo>();
            demo.InvokeAsync();
            Console.WriteLine("continue...");
            Console.Read();
        }
    }

    [Foobar]
    public class Demo
    {
        //[NonInterceptable(typeof(FoobarAttribute))]
        public virtual Task InvokeAsync()
        {
            Console.WriteLine("Invoke...");
            return Task.CompletedTask;
        }
    }

    public class FoobarInterceptor
    {
        private InterceptDelegate _next;

        public FoobarInterceptor(InterceptDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            await Task.Delay(1000);
            Console.WriteLine("Intercept...");
            await _next(invocationContext);
        }
    }

    public class FoobarAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<FoobarInterceptor>(this.Order);
        }
    }
}
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        var clock1 = new ServiceCollection()
    //          .AddLogging(factory=> factory.AddConsole())
    //          .AddMemoryCache()
    //          .AddSingleton<ISystomClock, SystomClock>()
    //          .AddInterception()
    //          .BuildServiceProvider()
    //          .GetRequiredService<IInterceptable<ISystomClock>>()
    //          .Proxy;

    //        var method = typeof(ISystomClock).GetMethod("GetCurrentTime");
    //        var field = clock1.GetType().GetField("_interceptors", BindingFlags.NonPublic | BindingFlags.Instance);
    //        var decoration = field.GetValue(clock1) as InterceptorDecoration;
    //        var interceptor = decoration.GetInterceptor(method);

    //        for (int i = 0; i < 5; i++)
    //        {
    //            Console.WriteLine($"Current time: {clock1.GetCurrentTime(DateTimeKind.Local)}");
    //            Task.Delay(1000).Wait();
    //        }

    //        for (int i = 0; i < 5; i++)
    //        {
    //            Console.WriteLine($"Current time: {clock1.GetCurrentTime(DateTimeKind.Utc)}");
    //            Task.Delay(1000).Wait();
    //        }


    //        var clock2 = new ServiceCollection()
    //          .AddMemoryCache()
    //          .AddSingleton<ISystomClock, SystomClock>()
    //          .BuilderInterceptableServiceProvider()
    //          .GetRequiredService<ISystomClock>();

    //        for (int i = 0; i < 5; i++)
    //        {
    //            Console.WriteLine($"Current time: {clock2.GetCurrentTime(DateTimeKind.Local)}");
    //            Task.Delay(1000).Wait();
    //        }

    //        for (int i = 0; i < 5; i++)
    //        {
    //            Console.WriteLine($"Current time: {clock2.GetCurrentTime(DateTimeKind.Utc)}");
    //            Task.Delay(1000).Wait();
    //        }
    //    }
    //}

