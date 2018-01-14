using System;

namespace Dora.DynamicProxy
{
    public interface IInterceptorCollector
    {
        InterceptorDecoration GetInterceptors(Type typeToIntercept, Type targetType);
        InterceptorDecoration GetInterceptors(Type typeToIntercept);
    }
}
