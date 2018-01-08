using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class InterfaceInterceptingProxyClassGeneratorFixture
    {
        [Fact]
        public void ReturnGeneralType()
        {
            InterceptorDelegate interceptor = next => (async context =>
            {
                context.Arguments[0] = (int)context.Arguments[0] + 1;  
                await next(context);
            });
            var method = ReflectionUtility.GetMethod<ICalculator>(_ => _.Add(1, 2));
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = new InterfaceInterceptingProxyClassGenerator();
            var proxyType = generator.GenerateProxyClass(typeof(ICalculator), decoration);
            var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), decoration);
            Assert.Equal(4, proxy.Add(1, 2));
        }

        //[Fact]
        //public void RefParameter()
        //{
        //    InterceptorDelegate interceptor = next => (async context =>
        //    {
        //        context.Arguments[0] = (int)context.Arguments[0] + 1;
        //        await next(context);
        //    });
        //    int x = 1;
        //    int y = 2;
        //    var method = ReflectionUtility.GetMethod<ICalculator>(_ => _.Substract(ref x, ref y));
        //    var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
        //    var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
        //    var generator = new InterfaceInterceptingProxyClassGenerator();
        //    var proxyType = generator.GenerateProxyClass(typeof(ICalculator), decoration);
        //    var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), decoration);
        //    Assert.Equal(0, proxy.Substract(ref x, ref y));
        //}

        public interface ICalculator
        {
            int Add(int x, int y);
            //int Substract(ref int x, ref int y);
        }

        public class Calculator : ICalculator
        {
            public int Add(int x, int y)
            {
                return x + y;
            }

            //public int Substract(ref int x, ref int y)
            //{
            //    return x - y;
            //}
        }
    }
}
