using Dora.Primitives;
using System;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// <see cref="IInterceptorRegistrationProvider"/> specific extension methods.
    /// </summary>
    public static class InterceptorRegistrationProviderExtensions
    {
        /// <summary>
        /// Determines whether to intercept specified type.
        /// </summary>
        /// <param name="interceptorRegistrationProvider">The interceptor registration provider.</param>
        /// <param name="type">The target type to check.</param>
        /// <returns>A Boolean value indicating whether to intercept specified type. </returns>
        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, Type type)
        {
            Guard.ArgumentNotNull(interceptorRegistrationProvider, nameof(interceptorRegistrationProvider));
            Guard.ArgumentNotNull(type, nameof(type));
            return interceptorRegistrationProvider.GetRegistrations(type).Any();
        }

        /// <summary>
        /// Determines whether to intercept specified method.
        /// </summary>
        /// <param name="interceptorRegistrationProvider">The interceptor registration provider.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <returns>A Boolean value indicating whether to intercept specified method. </returns>
        public static bool WillIntercept(this IInterceptorRegistrationProvider interceptorRegistrationProvider, MethodInfo  targetMethod)
        {
            Guard.ArgumentNotNull(interceptorRegistrationProvider, nameof(interceptorRegistrationProvider));
            Guard.ArgumentNotNull(targetMethod, nameof(targetMethod));
            return interceptorRegistrationProvider.GetRegistrations(targetMethod.DeclaringType).Any(it => it.TargetMethod == targetMethod);
        }
    }
}
