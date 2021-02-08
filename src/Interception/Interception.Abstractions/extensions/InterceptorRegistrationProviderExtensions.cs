using System;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    public static class InterceptorRegistrationProviderExtensions
    {
        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, Type type)
        {
            return interceptorRegistrationProvider.GetRegistrations(type).Any();
        }

        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, MethodInfo  methodInfo)
        {
            return interceptorRegistrationProvider.GetRegistrations(methodInfo.DeclaringType).Any(it => it.Target == methodInfo);
        }
    }
}
