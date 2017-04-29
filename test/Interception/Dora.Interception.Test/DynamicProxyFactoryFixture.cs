using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class DynamicProxyFactoryFixture
    {
        private static string _flag;

        [Fact]
        public void InterceptForInterface()
        {
            _flag = null;
            var serviceProvider = new ServiceCollection().AddScoped<IService, Service>()
                .BuilderInterceptableServiceProvider(builder => builder.SetDynamicProxyFactory());
            var service = serviceProvider.GetRequiredService<IService>();
            service.Invoke();
            Assert.Equal("123", _flag);
        }

        [Fact]
        public void InterceptForClass()
        {
            _flag = null;
            var serviceProvider = new ServiceCollection().AddScoped<BaseService, SubService>()
                .BuilderInterceptableServiceProvider(builder => builder.SetDynamicProxyFactory());
            var service = serviceProvider.GetRequiredService<BaseService>();
            service.Invoke();
            Assert.Equal("123", _flag);
        }

        public interface IService
        {
            void Invoke();
        }

        public class Service : IService
        {
            [Foobar]
            public void Invoke()
            {
               
            }
        }

        public class BaseService
        {
            public virtual void Invoke()
            { }
        }

        public class SubService : BaseService
        {
            [Foobar]
            public override void Invoke()
            {
                base.Invoke();
            }
        }


        private class FoobarInterceptor
        {
            private readonly InterceptDelegate _next;
            public FoobarInterceptor(InterceptDelegate next)
            {
                _next = next;
            }
            public async Task InvokeAsync(InvocationContext context)
            {
                _flag = "123";
                await _next(context);
            }
        }

        private class FoobarAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<FoobarInterceptor>(this.Order);
            }
        }

    }
}
