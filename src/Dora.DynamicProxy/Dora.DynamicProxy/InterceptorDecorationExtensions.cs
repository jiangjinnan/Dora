using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    public static class InterceptorDecorationExtensions
    {
        public static bool Contains(this InterceptorDecoration interceptorDecoration ,MethodInfo method)
        {
            Guard.ArgumentNotNull(interceptorDecoration, nameof(interceptorDecoration));
            Guard.ArgumentNotNull(method, nameof(method));
            return interceptorDecoration.MethodBasedInterceptors.ContainsKey(method);
        }
    }
}
