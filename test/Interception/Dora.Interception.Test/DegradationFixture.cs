//using Dora.DynamicProxy;
//using Microsoft.Extensions.DependencyInjection;
//using System.Threading.Tasks;
//using Xunit;

//namespace Dora.Interception.Test
//{
//    public class DegradationFixture
//    {
//        private static string _flag = "";

//        [Fact]
//        public async void Test()
//        {
//            _flag = "";
//            var foobar = new ServiceCollection()
//                .AddSingleton<Foobar, Foobar>()
//                .BuildInterceptableServiceProvider()
//                .GetRequiredService<Foobar>();
//            var result = await foobar.InvokeAsync();
//            Assert.Equal("Target", result);
//            Assert.Equal("TargetFoobar", _flag);
//        }

//        public class Foobar: IFoobar
//        {
//            [FoobarInterceptor]
//            public virtual async Task<string> InvokeAsync ()
//            {
//                await Task.Delay(1000);
//                _flag += "Target";
//                return _flag;
//            }
//        }

//        public class FoobarInterceptorAttribute : InterceptorAttribute
//        {
//            public async Task InvokeAsync(InvocationContext context)
//            {
//                await context.ProceedAsync();
//                await Task.Delay(1000);
//                _flag += "Foobar";
//            }
//            public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
//        }

//        public interface IFoobar
//        {
//            Task<string> InvokeAsync();
//        }

//        [Fact]
//        public async void TestClassGenerator()
//        {
//            var foobar = new ServiceCollection()
//                .AddInterception()
//                .AddSingleton(provider =>
//                {
//                    var resolver = provider.GetRequiredService<IInterceptorResolver>();
//                    var interceptors = resolver.GetInterceptors(typeof(IFoobar), typeof(Foobar));
//                    var proxyType = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(IFoobar), interceptors).GenerateProxyType();
//                    var proxy = ActivatorUtilities.CreateInstance(provider, proxyType, ActivatorUtilities.CreateInstance(provider, typeof(Foobar)), interceptors);
//                    return (IFoobar)proxy;
//                })
//                .BuildServiceProvider()
//                .GetRequiredService<IFoobar>();
//            var result = await foobar.InvokeAsync();
//            Assert.Equal("Target", result);
//            Assert.Equal("TargetFoobar", _flag);
//        }
//    }
//}
