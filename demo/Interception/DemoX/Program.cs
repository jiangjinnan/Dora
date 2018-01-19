using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DemoX
{
    class Program
    {
        static void Main(string[] args)
        {
            var demo = new ServiceCollection()
                     .AddSingleton<Demo, Demo1>()
                     .BuildInterceptableServiceProvider()
                     .GetRequiredService<Demo>();

            demo.GetValueAsync();
            //Console.WriteLine("Set...");
            //demo.Value = new object();
            //Console.WriteLine("Get...");
            //var value = demo.Value;
            Console.Read();
        }
    }

    public class Demo
    {
        [Foobar]
        public virtual Task InvokeAsync()
        {
            Console.WriteLine("Target method is invoked.");
            return Task.CompletedTask;
        } 
      
        public virtual object Value { [Foobar]get; set; }


        [Foobar]
        public virtual Task<string> GetValueAsync()
        {
            return Task.FromResult("Foobar");
        }
    }

    public class Demo1 : Demo
    {  
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
