using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using Dora.DynamicProxy;

namespace Dora.Interception.Test
{
    public class InterceptorChainBuilderFixture
    {
        [Fact]
        public void Construct_Arguments_Not_Allow_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new InterceptorChainBuilder(null));
        }

        [Fact]
        public void Construct()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            Assert.Same(serviceProvider, new InterceptorChainBuilder(serviceProvider).ServiceProvider);
        }

        [Fact]
        public void Use_Arguments_Not_Allow_Null()
        {
            var builder = new InterceptorChainBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentNullException>(() => builder.Use(null, 1));
        }

        [Fact]
        public async void Use_And_Build()
        {
            var builder = new InterceptorChainBuilder(new ServiceCollection().BuildServiceProvider());
            string value = "";

            InterceptDelegate interceptor1(InterceptDelegate next) => (async context => { value += "1"; await next(context); });
            InterceptDelegate interceptor2(InterceptDelegate next) => (async context => { value += "2"; await next(context); });
            InterceptDelegate interceptor3(InterceptDelegate next) => (context => { value += "3"; return Task.CompletedTask; });
            InterceptDelegate interceptor4(InterceptDelegate next) => (async context => { value += "4"; await next(context); });

            builder
               .Use(interceptor2, 2)
               .Use(interceptor4, 4)
               .Use(interceptor1, 1)
               .Use(interceptor3, 3);

            var chain = builder.Build();
            await chain(context => Task.CompletedTask)(new FoobarInvocationContext());
            Assert.Equal("123", value);
        }

        [Fact]
        public void New()
        {
            var builder = new InterceptorChainBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Same(builder.ServiceProvider, builder.New().ServiceProvider);
        }

        private class FoobarInvocationContext : InvocationContext
        {
            public override MethodInfo Method => throw new NotImplementedException();

            public override object Proxy => throw new NotImplementedException();

            public override object Target => throw new NotImplementedException();

            public override object[] Arguments => throw new NotImplementedException();

            public override object ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override IDictionary<string, object> Properties => throw new NotImplementedException();
        }
    }
}
