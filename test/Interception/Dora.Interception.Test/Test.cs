using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class Test
    {
        [Fact]
        public  void Invoke()
        {
            var demo = new ServiceCollection()
                    .AddSingleton<Demo, Demo>()
                    .BuildInterceptableServiceProvider()
                    .GetRequiredService<Demo>();
            Console.WriteLine("Set...");
            demo.Value = new object();      
        }

        public class Demo
        {
            [Foobar]
            public virtual Task InvokeAsync()
            {
                Console.WriteLine("Target method is invoked.");
                return Task.CompletedTask;
            }

            [Foobar]
            public virtual object Value { get; set; }
        }

        public class FoobarInterceptor
        {
            private InterceptDelegate _next;

            public FoobarInterceptor(InterceptDelegate next)
            {
                _next = next;
            }
            public async Task InvokeAsync(InvocationContext context)
            {
                Console.WriteLine("Interception task starts.");
                await Task.Delay(1000);
                Console.WriteLine("Interception task completes.");
                await _next(context);
            }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
        public class FoobarAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptor>(this.Order);
            }
        }
    }
}
