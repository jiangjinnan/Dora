using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInterceptor : IInterceptor
    {
        private InterceptorDelegate  _interceptor;

        public DynamicProxyInterceptor(InterceptorDelegate inteceptor)
        {
            _interceptor = inteceptor;
        }
        public void Intercept(IInvocation invocation)
        {
            InterceptDelegate next = async context => await ((DynamicProxyInvocationContext)context).ProceedAsync();
            _interceptor(next)(new DynamicProxyInvocationContext(invocation)).Wait();
        }
    }
}