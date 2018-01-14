using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    public class TransparentProxyDynamicProxyGenerator : IInstanceDynamicProxyGenerator
    {
        public bool CanIntercept(Type type)
        {
            throw new NotImplementedException();
        }

        public object Wrap(Type type, object target, InterceptorDecoration interceptors)
        {
            throw new NotImplementedException();
        }
    }
}
