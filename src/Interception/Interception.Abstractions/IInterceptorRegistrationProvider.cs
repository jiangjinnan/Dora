using System;
using System.Collections.Generic;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get interceptor registrations.
    /// </summary>
    public interface IInterceptorRegistrationProvider
    {
        /// <summary>
        /// Gets the interceptor registrations.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <returns>The interceptor registrations.</returns>
        IEnumerable<InterceptorRegistration> GetRegistrations(Type targetType);
    }
}
