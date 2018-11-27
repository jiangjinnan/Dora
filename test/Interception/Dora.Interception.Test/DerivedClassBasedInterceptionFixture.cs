using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Dora.Interception.Test.InterceptableServiceProvider;

namespace Dora.Interception.Test
{
    public class DerivedClassBasedInterceptionFixture
    {
        private static string _flag4Interceptor;
        private static string _flag;

        [Fact]
        public async void Invoke()
        {
            var bar = new ServiceCollection()
                .AddScoped<IFoobar, Bar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();
            await bar.Invoke1();
            Assert.Equal("Foo", _flag);
            await bar.Invoke2();
            Assert.Equal("Bar", _flag);

            var baz = new ServiceCollection()
                .AddScoped<IFoobar, Baz>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();
            await baz.Invoke1();
            Assert.Equal("Baz", _flag);
            await baz.Invoke2();
            Assert.Equal("Bar", _flag);
        }

        public interface IFoobar
        {
            Task Invoke1();
            Task Invoke2();
        }

        [Foobar]
        public abstract class Foo : IFoobar
        {
            public virtual Task Invoke1()
            {
                _flag = "Foo";
                return Task.CompletedTask;
            }

            public abstract Task Invoke2();
        }

        public class Bar : Foo
        {
            public override Task Invoke2()
            {
                _flag = "Bar";
                return Task.CompletedTask;
            }
        }

        public class Baz : Bar
        {
            public override Task Invoke1()
            {
                _flag = "Baz";
                return Task.CompletedTask;
            }
        }

        public class FoobarAttribute : InterceptorAttribute
        {
            public Task InvokeAsync(InvocationContext invocationContext)
            {
                _flag4Interceptor = "FoobarAttribute";
                return invocationContext.ProceedAsync();
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarAttribute>(Order);
            }
        }
    }
}
