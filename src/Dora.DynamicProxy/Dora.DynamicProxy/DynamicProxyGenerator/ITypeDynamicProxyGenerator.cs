using System;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents the generator to generate the interceptable dynamic proxy.
    /// </summary>
    public interface ITypeDynamicProxyGenerator : IDynamicProxyGenerator
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
