using System;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Representing which interceptors are applied to which members of a type to intercept.
    /// </summary>
    public interface IInterceptorRegistry
    {
        /// <summary>
        /// Gets a value indicating whether there is no interceptor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if no interceptor is applied.; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the interceptor based on specified method.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> decorated with interceptor.</param>
        /// <returns>The <see cref="InterceptorDelegate"/> representing the interceptor decorated with specified method.</returns>    
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodInfo"/> is null.</exception>
        InterceptorDelegate GetInterceptor(MethodInfo methodInfo);

        /// <summary>
        /// Determines whether the specified method information is interceptable.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>
        ///   <c>true</c> if the specified method information is interceptable; otherwise, <c>false</c>.
        /// </returns>   
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodInfo"/> is null.</exception>
        bool IsInterceptable(MethodInfo methodInfo);

        /// <summary>
        /// Get target method.
        /// </summary>
        /// <param name="methodInfo">The method to intercept.</param>
        /// <returns>The target method.</returns>
        MethodInfo GetTargetMethod(MethodInfo methodInfo);
    }
}