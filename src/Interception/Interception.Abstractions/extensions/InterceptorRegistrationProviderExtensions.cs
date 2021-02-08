using System;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    public static class InterceptorRegistrationProviderExtensions
    {
        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, Type type)
        {
            return interceptorRegistrationProvider.GetRegistrations(type).Any(it => it.Target.DeclaringType == type || it.Target.DeclaringType.IsAssignableFrom(type));
        }

        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, MethodInfo  methodInfo)
        {
            return interceptorRegistrationProvider.GetRegistrations(methodInfo.DeclaringType).Any(it => it.Target == methodInfo);
        }
    }
}
