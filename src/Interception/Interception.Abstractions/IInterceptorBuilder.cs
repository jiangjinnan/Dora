using System.Collections.Generic;

namespace Dora.Interception
{
    /// <summary>
    /// Builder to build an interceptor (chain) with specified sorted interceptor registrations.
    /// </summary>
    public interface IInterceptorBuilder
    {
        /// <summary>
        /// Builds an interceptor (chain) with specified sorted interceptor registrations.
        /// </summary>
        /// <param name="registrations">The sorted interceptor registrations.</param>
        /// <returns>The built interceptor (chain).</returns>
        IInterceptor Build(IEnumerable<InterceptorRegistration> registrations);
    }
}
