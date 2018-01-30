using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class InterceptableServiceProvider
    {
        private static Action<InvocationContext> _intercept;
        private static Action _invokeTarget; 

        [Fact]
        public async void InterceptForInterface()
        {
            var flag1 = "";
            var flag2 = "";
            _intercept = _ => flag1 = "Foobar";
            _invokeTarget = ()=> flag2 = "Foobar";

            var baz = new ServiceCollection()  
                .AddSingleton<IFoo, Foo>()
                .AddSingleton<IBar, Bar>()
                .AddSingleton<IBaz, Baz>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IBaz>();
            await baz.InvokeAsync();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2); 
        }

        [Fact]
        public async void InterceptForVirtualMethod()
        {
            var flag1 = "";
            var flag2 = "";
            _intercept = _ => flag1 = "Foobar";
            _invokeTarget = () => flag2 = "Foobar";

            var gux = new ServiceCollection()
                .AddSingleton<IFoo, Foo>()
                .AddSingleton<IBar, Bar>()
                .AddSingleton<Gux, Gux>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Gux>();
            await gux.InvokeAsync();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);
        }


        public interface IBaz
        {
            Task InvokeAsync();
        }

        public class Baz : IBaz
        {
            [Foobar("Foobar")]
            public Task InvokeAsync()
            {
                _invokeTarget();
                return Task.CompletedTask;
            }
        }

        public class Gux
        {
            [Foobar("Foobar")]
            public virtual Task InvokeAsync()
            {
                _invokeTarget();
                return Task.CompletedTask;
            }
        }


        public class FoobarInterceptor
        {                                              
            public IFoo Foo { get; }
            public IBar Bar { get; }
            public string Flag { get; }
            public FoobarInterceptor(IFoo foo,IBar bar,string flag)
            {                  
                this.Foo = foo;
                this.Bar = bar;
                this.Flag = flag;
            }

            public Task InvokeAsync(InvocationContext context)
            {
                _intercept(context);
                return context.ProceedAsync();
            }
        }

        [AttributeUsage(AttributeTargets.Property| AttributeTargets.Method)]
        public class FoobarAttribute : InterceptorAttribute
        {
            public string Flag { get; }

            public FoobarAttribute(string flag)
            {
                this.Flag = flag;
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptor>(this.Order, this.Flag);
            }
        }

        public interface IFoo
        { }

        public interface IBar
        { }

        public class Foo : IFoo { }

        public class Bar : IBar { }

    }
}
