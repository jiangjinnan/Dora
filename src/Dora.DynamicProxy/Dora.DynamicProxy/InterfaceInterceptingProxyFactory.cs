using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    public class InterfaceInterceptingProxyFactory : IInterceptingProxyFactory
    {
        public object CreateInterceptingProxy(Type type, object target, IDictionary<MethodInfo, InterceptorDelegate> interceptors)
        {
            throw new NotImplementedException();
        }
    }
}
