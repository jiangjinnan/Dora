using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class OpenGenericTypeFixture
    {
        private static Action _action = ()=> { };

        [Fact]
        public void GetService()
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
            Assert.Equal("Foobar", flag);
            
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
        public class Foobar<TFoo, TBar>: IFoobar<TFoo, TBar>
            where TFoo : IFoo
            where TBar : IBar
        {
            public Foobar(TFoo foo, TBar bar)
            {
                this.Foo = foo;
                this.Bar = bar;
            }
           public TFoo Foo { get; }
           public TBar Bar { get; }
        }

        [Foobar]
        public class FoobarInterceptor
        {
            private InterceptDelegate _next;
            public FoobarInterceptor(InterceptDelegate next)
            {
                _next = next;
            }

            public Task InvokeAsync(InvocationContext context)
            {
                _action();
                return _next(context);
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
}
