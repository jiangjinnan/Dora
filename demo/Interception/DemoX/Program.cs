using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DemoX
{
    public class Program
    {
        private static int _flag = 0;

        static void Main()
        {
            Intercept1();
            Intercept2();
            Intercept3();
        }

        private static void Intercept1()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .AddSingleton<Foobar>()
                .AddInterception()
                .BuildServiceProvider();

            var proxy1 = serviceProvider.GetRequiredService<IInterceptable<IFoobar>>().Proxy;
            _flag = 0;
            proxy1.Invoke();
            Debug.Assert(_flag == 1);

            var proxy2 = serviceProvider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            _flag = 0;
            proxy2.Invoke();
            Debug.Assert(_flag == 1);
        }

        private static void Intercept2()
        {
            var serviceProvider = new ServiceCollection()
                .AddInterception()
                //.AddInterceptableSingleton<IFoobar, Foobar>()
                //.AddInterceptableSingleton<Foobar>()
                .BuildServiceProvider();

            var proxy1 = serviceProvider.GetRequiredService<IFoobar>();
            _flag = 0;
            proxy1.Invoke();
            Debug.Assert(_flag == 1);

            var proxy2 = serviceProvider.GetRequiredService<Foobar>();
            _flag = 0;
            proxy2.Invoke();
            Debug.Assert(_flag == 1);
        }

        private static void Intercept3()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .AddSingleton<Foobar>()
                .BuildInterceptableServiceProvider();

            var proxy1 = serviceProvider.GetRequiredService<IFoobar>();
            _flag = 0;
            proxy1.Invoke();
            Debug.Assert(_flag == 1);

            var proxy2 = serviceProvider.GetRequiredService<Foobar>();
            _flag = 0;
            proxy2.Invoke();
            Debug.Assert(_flag == 1);
        }

        public interface IFoobar
        {
            void Invoke();
        }

        public class Foobar : IFoobar
        {
            [FakeInterceptor]
            public virtual void Invoke() { }
        }

        public class FakeInterceptorAttribute : InterceptorAttribute
        {
            public Task InvokeAsync(InvocationContext context)
            {
                _flag = 1;
                return context.ProceedAsync();
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use(this, Order);
            }
        }
    }
}
