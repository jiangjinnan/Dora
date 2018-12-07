using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class NullReturnValueFixture
    {
        [Fact]
        public async void Test()
        {
            var foobar = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();
            Assert.Null(await foobar.Invoke());
        }

        public class Foobar
        {
            [NilInterceptor]
            public virtual Task<string> Invoke()
            {
                return Task.FromResult("Foobar");
            }
        }
    }

   

    public class NilInterceptorAttribute: InterceptorAttribute
    {
        public Task InvokeAsync(InvocationContext context)
        {
            return Task.CompletedTask;
        }
        public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
    }
}
