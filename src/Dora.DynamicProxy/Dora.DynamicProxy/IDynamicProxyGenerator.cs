using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    public interface IDynamicProxyGenerator
    {
        bool CanIntercept(Type type);
    }

    public interface IInstanceDynamicProxyGenerator : IDynamicProxyGenerator
    {
        object Wrap(Type type, object target, InterceptorDecoration interceptors);
    }

    public interface ITypeDynamicProxyGenerator: IDynamicProxyGenerator
    {
        object Create(Type type, InterceptorDecoration interceptors, IServiceProvider serviceProvider);
    }
}
