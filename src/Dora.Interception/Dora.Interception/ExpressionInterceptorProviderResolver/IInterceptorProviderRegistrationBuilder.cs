using System;
using System.Collections.Generic;

namespace Dora.Interception
{
    /// <summary>
    /// Defines method to build interceptor provider specific interception policy.
    /// </summary>
    public interface IInterceptorProviderRegistrationBuilder
    {
        /// <summary>
        /// Builds interceptor provider specific interception policy.
        /// </summary>
        /// <returns>A collection of <see cref="TargetRegistration"/>.</returns>
        IEnumerable<TargetRegistration> Build();


        /// <summary>
        /// Builds the specifc target type specific interception policy.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="configure">The delegate to configure specified target type based interception policy.</param>
        /// <returns>The current <see cref="IInterceptorProviderRegistrationBuilder"/></returns>
        IInterceptorProviderRegistrationBuilder Target<T>(Action<ITargetRegistrationBuilder<T>> configure);
    }
}
