using System;
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
        /// <param name="type">The type to be checked for interception.</param>
        /// <returns>A <see cref="Nullable{Boolean}"/> indicating whether the specified type should be intercepted.</returns>
        bool? WillIntercept(Type type);

        /// <summary>
        /// Determine whether the specified method should be intercepted.
        /// </summary>
        /// <param name="method">The method to be checked for interception.</param>
        /// <returns>A <see cref="Nullable{Boolean}"/> indicating whether the specified method should be intercepted.</returns>
        bool? WillIntercept(MethodInfo method);

        /// <summary>
        /// Gets the interceptor providers applied to the specified type.
        /// </summary>
        /// <param name="type">The type to which the interceptor providers are applied to.</param>
        /// <returns>The interceptor providers applied to the specified type.</returns>
        IInterceptorProvider[] GetInterceptorProvidersForType(Type type);

        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="method">The method to which the interceptor providers are applied to.</param>
        /// <returns>The interceptor providers applied to the specified method.</returns>
        IInterceptorProvider[] GetInterceptorProvidersForMethod(MethodInfo method);


        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="property">The property to which the interceptor providers are applied to.</param>
        /// <param name="propertyMethod">The property's GET or SET method.</param>
        /// <returns>
        /// The interceptor providers applied to the specified method.
        /// </returns>
        IInterceptorProvider[] GetInterceptorProvidersForProperty(PropertyInfo property, PropertyMethod propertyMethod);
    }
}
