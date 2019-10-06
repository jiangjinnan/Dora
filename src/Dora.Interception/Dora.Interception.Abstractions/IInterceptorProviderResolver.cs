using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    ///Defines methods to resolve interceptor providers applied to specified type and method. 
    /// </summary>
    public interface IInterceptorProviderResolver
    {
        /// <summary>
        /// Determine whether the specified type should be intercepted.
        /// </summary>
        /// <param name="targetType">The type to be checked for interception.</param>
        /// <returns>
        /// A <see cref="Nullable{Boolean}" /> indicating whether the specified type should be intercepted.
        /// </returns>
        bool? WillIntercept(Type targetType);

        /// <summary>
        /// Determine whether the specified method should be intercepted.
        /// </summary>
        /// <param name="targetType">The type of of the method to be checked for interception.</param>
        /// <param name="method">The method to be checked for interception.</param>
        /// <returns>
        /// A <see cref="Nullable{Boolean}" /> indicating whether the specified method should be intercepted.
        /// </returns>
        public bool? WillIntercept(Type targetType, MethodInfo method);

        /// <summary>
        /// Determine whether the specified property should be intercepted.
        /// </summary>
        /// <param name="targetType">The type of of the property to be checked for interception.</param>
        /// <param name="property">The property to be checked for interception.</param>
        /// <returns>
        /// A <see cref="Nullable{Boolean}" /> indicating whether the specified method should be intercepted.
        /// </returns>
        bool? WillIntercept(Type targetType, PropertyInfo property);

        /// <summary>
        /// Gets the interceptor providers applied to the specified type.
        /// </summary>
        /// <param name="targetType">The type to which the interceptor providers are applied to.</param>
        /// <param name="excludedInterceptorProviders">Excluded interceptor provider types.</param>
        /// <returns>The interceptor providers applied to the specified type.</returns>
        IInterceptorProvider[] GetInterceptorProvidersForType(Type targetType, out ISet<Type> excludedInterceptorProviders);

        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetMethod">The method to which the interceptor providers are applied to.</param>
        /// <param name="excludedInterceptorProviders">Excluded interceptor provider types.</param>
        /// <returns>
        /// The interceptor providers applied to the specified method.
        /// </returns>
        IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo targetMethod, out ISet<Type> excludedInterceptorProviders);


        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetProperty">The property to which the interceptor providers are applied to.</param>
        /// <param name="excludedInterceptorProviders">Excluded interceptor provider types.</param>
        /// <param name="getOrSet">The property's GET or SET method.</param>
        /// <returns>
        /// The interceptor providers applied to the specified method.
        /// </returns>
        IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo targetProperty, PropertyMethod getOrSet, out ISet<Type> excludedInterceptorProviders);
    }
}
