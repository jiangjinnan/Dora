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
        private static Action _action = () => { };
        static void Main(string[] args)
        {
            var foobar = new ServiceCollection()
                 .AddSingleton<IFoo, Foo>()
                 .AddSingleton<IBar, Bar>()
                 .AddSingleton(typeof(IFoobar<,>), typeof(Foobar<,>))
                 .BuildInterceptableServiceProvider()
                 .GetRequiredService<IFoobar<IFoo, IBar>>();
            var flag = "";
            _action = () => flag = "Foobar";
            var foo = foobar.Foo;
            Debug.Assert("Foobar" == flag);
        }
        public interface IFoo { }
        public interface IBar { }
        public interface IFoobar<TFoo, TBar>
            where TFoo : IFoo
            where TBar : IBar
        {
            TFoo Foo { get; }
            TBar Bar { get; }
        }
        public class Foo : IFoo { }
        public class Bar : IBar { }

        [Foobar]
        public class Foobar<TFoo, TBar> : IFoobar<TFoo, TBar>
            where TFoo : IFoo
            where TBar : IBar
        {
            public Foobar(TFoo foo, TBar bar)
            {
                Foo = foo;
                Bar = bar;
            }
            public TFoo Foo { get; }
            public TBar Bar { get; }
        }   
      
        public class FoobarInterceptor
        {
            public Task InvokeAsync(InvocationContext context)
            {
                _action();
                return context.ProceedAsync();
            }
        }

        public class FoobarAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptor>(Order);
            }
        }

    }
}
