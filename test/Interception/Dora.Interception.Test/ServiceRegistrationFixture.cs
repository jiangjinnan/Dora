//using Dora.DynamicProxy;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace Dora.Interception.Test
//{
//    public class ServiceRegistrationFixture
//    {
//        [Fact]
//        public void Intercept()
//        {
//            var provider = new ServiceCollection()
//                .AddInterception()
//                .AddInterceptableSingleton< IFoobar, Foobar>()
//                .AddInterceptableSingleton<Foobar>()
//                .BuildServiceProvider();

//            var proxy1 = provider.GetRequiredService<IFoobar>();
//            _flag = 0;
//            proxy1.Invoke();
//            Assert.Equal(1, _flag);

//            var proxy2 = provider.GetRequiredService<Foobar>();
//            _flag = 0;
//            proxy2.Invoke();
//            Assert.Equal(1, _flag);
//        }


//        private static int _flag;

//        public interface IFoobar
//        {
//            void Invoke();
//        }

//        public class Foobar : IFoobar
//        {
//            [FakeInterceptor]
//            public virtual void Invoke() { }
//        }

//        public class FakeInterceptorAttribute : InterceptorAttribute
//        {
//            public Task InvokeAsync(InvocationContext context)
//            {
//                _flag = 1;
//                return context.ProceedAsync();
//            }
//            public override void Use(IInterceptorChainBuilder builder)
//            {
//                builder.Use(this, Order);
//            }
//        }
//    }
//}
