using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class NonInterceptableAttributeFixture
    {
        [Fact]
        public void Intercept4Interface()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<IFoobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<IFoobar>();

            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke1();
            Assert.Equal("", FakeInterceptorAttribute.GetResult<IFoobar>());

            FoobarInterceptorAttribute.Result = 0;
            foobar.Invoke1();
            Assert.Equal(0, FoobarInterceptorAttribute.Result);


            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke2();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<IFoobar>());

            FoobarInterceptorAttribute.Result = 0;
            foobar.Invoke2();
            Assert.Equal(1, FoobarInterceptorAttribute.Result);

            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke3();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<IFoobar>());

            FoobarInterceptorAttribute.Result = 0;
            foobar.Invoke3();
            Assert.Equal(0, FoobarInterceptorAttribute.Result);

            foobar = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();

            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke1();
            Assert.Equal("", FakeInterceptorAttribute.GetResult<IFoobar>());

            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke2();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<IFoobar>());

            FakeInterceptorAttribute.Reset<IFoobar>();
            foobar.Invoke3();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<IFoobar>());
        }

        [Fact]
        public void Intercept4Class()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<Foobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<Foobar>();

            FakeInterceptorAttribute.Reset<Foobar>();
            foobar.Invoke1();
            Assert.Equal("", FakeInterceptorAttribute.GetResult<Foobar>());

            FakeInterceptorAttribute.Reset<Foobar>();
            foobar.Invoke2();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<Foobar>());

            foobar = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();

            FakeInterceptorAttribute.Reset<Foobar>();
            foobar.Invoke1();
            Assert.Equal("", FakeInterceptorAttribute.GetResult<Foobar>());

            FakeInterceptorAttribute.Reset<Foobar>();
            foobar.Invoke2();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<Foobar>());

            FakeInterceptorAttribute.Reset<Foobar>();
            foobar.Invoke3();
            Assert.Equal("1", FakeInterceptorAttribute.GetResult<Foobar>());
        }

        public interface IFoobar
        {
            void Invoke1();
            void Invoke2();
            void Invoke3();
        }

        [FakeInterceptor]
        [FoobarInterceptor]
        public class Foobar : IFoobar
        {
            [NonInterceptable]
            public virtual void Invoke1() { }

            public virtual void Invoke2() { }

            [NonInterceptable(typeof(FoobarInterceptorAttribute))]
            public virtual void Invoke3() { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class FoobarInterceptorAttribute : InterceptorAttribute
        {
            public static int Result;
            public Task InvokeAsync(InvocationContext invocationContext)
            {
                Result = 1;
                return invocationContext.ProceedAsync();
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use(this, Order);
            }
        }
    }
}
