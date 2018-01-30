using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDynamicProxyFactoryCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        Func<object, InterceptorDecoration, object> GetInstanceFactory(Type type, InterceptorDecoration interceptors);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        Func<InterceptorDecoration, IServiceProvider, object> GetTypeFactory(Type type, InterceptorDecoration interceptors);
    }
}
