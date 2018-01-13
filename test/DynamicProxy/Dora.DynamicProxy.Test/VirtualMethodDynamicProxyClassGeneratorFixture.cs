using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class VirtualMethodDynamicProxyClassGeneratorFixture
    {
        [Fact]
        public void Generate()
        {
            var flag1 = "";
            InterceptorDelegate interceptor = next => (context => { flag1 = "Foobar"; return next(context); });
            var proxy = CreateProxy<Calculator>(interceptor, _ => _.Add(0, 0));
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal("Foobar", flag1);
        }

        private T CreateProxy<T>(InterceptorDelegate interceptor, Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator =  DynamicProxyClassGenerator.CreateVirtualMethodGenerator(typeof(T), decoration);
            var proxyType = generator.GenerateProxyType();
            return (T)Activator.CreateInstance(proxyType, decoration);
        }


        public class Calculator
        {
            public virtual int Add(int x, int y)
            {
                return x + y;
            }
        }
    }
}
