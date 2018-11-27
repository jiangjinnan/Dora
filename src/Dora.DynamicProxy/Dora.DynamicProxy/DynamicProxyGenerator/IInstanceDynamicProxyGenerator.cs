using System;

namespace Dora.DynamicProxy
{
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
}
