using System;

namespace Dora.Interception
{
    /// <summary>
    /// Interceptable proxy class generator.
    /// </summary>
    public interface IInterceptableProxyGenerator
    {
        /// <summary>
        /// Generates the interceptable proxy class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <returns>The interceptable proxy type.</returns>
        Type Generate(Type serviceType, Type implementationType);
    }
}
