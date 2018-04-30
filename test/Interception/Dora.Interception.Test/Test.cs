using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class PerformanceTest
    {
        [Fact]
        public void ResolveInterceptors()
        {
            var fileName = @"..\..\..\ResolveInterceptors.txt";
            File.WriteAllText(fileName, "");
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var collector = new InterceptorResolver(new InterceptorChainBuilder(serviceProvider), new IInterceptorProviderResolver[] { new AttributeInterceptorProviderResolver()});
            var sw = Stopwatch.StartNew();
            collector.GetInterceptors(typeof(Demo));
            File.AppendAllLines(fileName, new string[] { sw.Elapsed.ToString() });

            sw.Restart();
            collector.GetInterceptors(typeof(Demo));
            File.AppendAllLines(fileName, new string[] { sw.Elapsed.ToString() });

            sw.Restart();
            collector.GetInterceptors(typeof(Demo));
            File.AppendAllLines(fileName, new string[] { sw.Elapsed.ToString() });

            Console.WriteLine(sw.Elapsed);
        }

        [Fact]
        public void CreateProxy()
        {
            var serviceProvider1 = new ServiceCollection()
                    .AddSingleton<IDemo, Demo>()
                    .BuildServiceProvider();
            var serviceProvider2 = new ServiceCollection()
                   .AddSingleton<IDemo, Demo>()
                   .BuildInterceptableServiceProvider();

            var fileName = @"..\..\..\CreateProxy.txt";
            File.WriteAllText(fileName, "");

            var sw = new Stopwatch();
            for (int index = 0; index < 100; index++)
            {
                sw.Restart();
                var demo = serviceProvider1.GetRequiredService<IDemo>();
                var time1 = sw.Elapsed;

                sw.Restart();
                demo = serviceProvider2.GetRequiredService<IDemo>();
                var time2 = sw.Elapsed;

                File.AppendAllLines(fileName, new string[] { time1.ToString(), time2.ToString(), "" });
            }

            var demo1 = serviceProvider1.GetRequiredService<IDemo>();
            var demo2 = serviceProvider2.GetRequiredService<IDemo>();

            for (int index = 0; index < 100; index++)
            {
                sw.Restart();
                demo1.Invoke();
                var time1 = sw.Elapsed;

                sw.Restart();
                demo2.Invoke();
                var time2 = sw.Elapsed;

                File.AppendAllLines(fileName, new string[] { time1.ToString(), time2.ToString(), "" });
            }
        }

        public interface IDemo
        {
            void Invoke();
        }

        public class Demo : IDemo
        {
            [FoobarInterceptor]
            public void Invoke()
            {

            }
        }

        public class FoobarInterceptorAttribute : InterceptorAttribute
        {
            public FoobarInterceptorAttribute()
            { }

            public Task InvokeAsync(InvocationContext context)
            {
                return context.ProceedAsync();
            }

            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptorAttribute>(this.Order);
            }
        }
    }
}
