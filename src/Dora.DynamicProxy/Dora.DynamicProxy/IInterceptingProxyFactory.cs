using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.DynamicProxy
{
    public interface IInterceptingProxyFactory
    {
        object CreateInterceptingProxy(Type type, object target, IDictionary<MethodInfo, InterceptorDelegate> interceptors);   
    } 
}
