using Castle.DynamicProxy;
using Dora.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInterceptor : AsyncInterceptorBase
    {
        private InterceptorDelegate  _interceptor;

        public DynamicProxyInterceptor(InterceptorDelegate inteceptor)
        {
            _interceptor = inteceptor;
        } 

        protected override  Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {                 
            var invocationContext = new DynamicProxyInvocationContext(invocation);
            InterceptDelegate next = context => proceed(invocation);    
            return _interceptor(next)(invocationContext);
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            var invocationContext = new DynamicProxyInvocationContext(invocation);
            InterceptDelegate next = context => proceed(invocation);
            await _interceptor(next)(invocationContext);
            return ((Task<TResult>)invocationContext.ReturnValue).Result;
        }  
    }
}