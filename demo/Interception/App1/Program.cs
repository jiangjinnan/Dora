using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        public static async Task Main()
        {
            var foobar = new ServiceCollection()
               .AddInterception()
               .AddSingletonInterceptable(typeof(Foo<,>), typeof(Foo<,>))
               .BuildServiceProvider()
               .GetRequiredService<Foo<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);

            //var clock = new ServiceCollection()
            //     .AddMemoryCache()
            //     .AddSingleton<ISystemClock, SystemClock>()
            //     .BuildInterceptableServiceProvider()
            //     .GetRequiredService<ISystemClock>();
            //for (int i = 0; i < 5; i++)
            //{
            //    Console.WriteLine(await clock.GetCurrentTimeAsync(DateTimeKind.Utc));
            //    await Task.Delay(1000);
            //}

            //clock = new ServiceCollection()
            //     .AddInterception()
            //     .AddMemoryCache()
            //     .AddSingletonInterceptable<ISystemClock, SystemClock>()
            //     .BuildServiceProvider()
            //     .GetRequiredService<ISystemClock>();
            //for (int i = 0; i < 5; i++)
            //{
            //    Console.WriteLine(await clock.GetCurrentTimeAsync(DateTimeKind.Utc));
            //    await Task.Delay(1000);
            //}
        }
    }

    public interface IFoobar<T1, T2>
    {
        Task Invoke<T>(T1 arg1, T2 arg2, T arg3);
    }

    public class Foo<T3, T4> : IFoobar<T3, T4>
    {
        [FakeInterceptor]
        public virtual Task Invoke<T>(T3 arg1, T4 arg2, T arg3) => Task.CompletedTask;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FakeInterceptorAttribute : InterceptorAttribute
    {
        public int Step { get; }
        public static string Result;

        public FakeInterceptorAttribute(int step = 1)
        {
            Step = step;
        }

        public static void Reset() => Result = "";
        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            Result += Step.ToString();
            try
            {
                await invocationContext.ProceedAsync();
            }
            catch (Exception ex)
            {
                throw new FakeException(nameof(FakeException), ex);
            }
        }
        public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
    }


    [Serializable]
    public class FakeException : Exception
    {
        public FakeException() { }
        public FakeException(string message) : base(message) { }
        public FakeException(string message, Exception inner) : base(message, inner) { }
        protected FakeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}