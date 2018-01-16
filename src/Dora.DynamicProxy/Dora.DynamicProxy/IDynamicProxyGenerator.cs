using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents the generator to generate the interceptable dynamic proxy.
    /// </summary>
    public interface IDynamicProxyGenerator
    {  
        /// <summary>
        /// Determines whether this specified type can be intercepted.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <returns>
        ///   <c>true</c> if the specified type can be intercepted; otherwise, <c>false</c>.
        /// </returns>
        bool CanIntercept(Type type);
    }

    /// <summary>
    /// Represents the generator to generate the interceptable dynamic proxy to wrap the specified target instance.
    /// </summary>
    public interface IInstanceDynamicProxyGenerator : IDynamicProxyGenerator
    {
        /// <summary>
        /// Creates a new interceptable dynamic proxy to wrap the specficied target instance.
        /// </summary>
        /// <param name="type">The interface type to intercept.</param>
        /// <param name="target">The target instance wrapped by the proxy.</param>
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.</param>
        /// <returns>The generated interceptable dynamic proxy.</returns>
        object Wrap(Type type, object target, InterceptorDecoration interceptors);
    }

    /// <summary>
    /// Represents the generator to generate the interceptable dynamic proxy.
    /// </summary>
    public interface ITypeDynamicProxyGenerator: IDynamicProxyGenerator
    {
        /// <summary>
        /// Creates a new interceptable dynamic proxy.
        /// </summary>
        /// <param name="type">The type to intercept.</param>   
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.</param>   
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> helping the creating object.</param>
        /// <returns>The interceptable dynamic proxy.</returns>
        object Create(Type type, InterceptorDecoration interceptors, IServiceProvider serviceProvider);
    }
}
