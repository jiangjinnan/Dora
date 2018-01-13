using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    public interface DynamicProxyBuilder
    {
        object Create(Type type, InterceptorDecoration interceptors);   
        object Wrap(Type type, object target, InterceptorDecoration interceptors);
    }
}
