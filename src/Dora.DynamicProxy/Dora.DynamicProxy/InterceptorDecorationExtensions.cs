using System.Reflection;

namespace Dora.DynamicProxy
{
    internal static class InterceptorDecorationExtensions
    {
        public static bool Contains(this InterceptorDecoration interceptorDecoration ,MethodInfo method)
        {
            Guard.ArgumentNotNull(interceptorDecoration, nameof(interceptorDecoration));
            Guard.ArgumentNotNull(method, nameof(method));
            return interceptorDecoration.MethodBasedInterceptors.ContainsKey(method);
        }
    }
}
