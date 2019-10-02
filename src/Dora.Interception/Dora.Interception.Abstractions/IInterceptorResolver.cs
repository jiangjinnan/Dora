using System;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Representing the collector to resolve the interceptors decorated with specified type.
    /// </summary>
    public interface IInterceptorResolver
    {
        /// <summary>
        /// Gets the interceptors decorated with the type of target instance.
        /// </summary>
        /// <param name="initerfaceType">The type to intercept.</param>
        /// <param name="targetType">Type of the target instance.</param>
        /// <returns>The <see cref="IInterceptorRegistry"/> representing the type members decorated with interceptors.</returns>
        IInterceptorRegistry GetInterceptors(Type initerfaceType, Type targetType);


        /// <summary>
        /// Gets the interceptors decorated with the specified type.
        /// </summary>
        /// <param name="targetType">The type to intercept.</param>  
        /// <returns>The <see cref="IInterceptorRegistry"/> representing the type members decorated with interceptors.</returns>
        IInterceptorRegistry GetInterceptors(Type targetType);
    }
}
                                                                                                       