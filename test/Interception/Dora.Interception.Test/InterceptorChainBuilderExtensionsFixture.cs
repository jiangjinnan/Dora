using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Dora;
using System.Threading.Tasks;
using System.Reflection;
using Dora.DynamicProxy;

namespace Dora.Interception.Test
{
    public class InterceptorChainBuilderExtensionsFixture
    {
        private static Action _intercept;


        [Fact]
        public async void Use()
        {
            var provider = new ServiceCollection().AddScoped<IService, Service>().BuildServiceProvider();
            var builder = new InterceptorChainBuilder(provider);
            string value = null;
            _intercept = () => value = Guid.NewGuid().ToString();
            var interceptorPipeline = builder.Use<FoobarInterceptor>(1, "abc").Build();
            await interceptorPipeline(context => Task.CompletedTask)(new FoobarInvocationContext());
            Assert.NotNull(value);
        }

        private class FoobarInterceptor
        {                                     
            public FoobarInterceptor(IService service, string argument)
            {                 
                if (null == service || null == argument)
                {
                    throw new InvalidOperationException();
                }
            }

            public async Task InvokeAsync(InvocationContext context)
            {
                _intercept();
                await context.ProceedAsync();
            }
        }
        private interface IService { }
        private class Service : IService { }
        private class FoobarInvocationContext : InvocationContext
        {
            public override MethodBase Method => throw new NotImplementedException();

            public override object Proxy => throw new NotImplementedException();

            public override object Target => throw new NotImplementedException();

            public override object[] Arguments => throw new NotImplementedException();

            public override object ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override IDictionary<string, object> ExtendedProperties => throw new NotImplementedException();
        }
    }
}
