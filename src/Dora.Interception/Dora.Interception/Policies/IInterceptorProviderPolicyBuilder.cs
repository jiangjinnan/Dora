using System;

namespace Dora.Interception.Policies
{
    /// <summary>
    /// Defines method to build interceptor provider specific interception policy.
    /// </summary>
    public interface IInterceptorProviderPolicyBuilder
    {
        /// <summary>
        /// Builds interceptor provider specific interception policy.
        /// </summary>
        /// <returns>A collection of <see cref="InterceptorProviderPolicy"/>.</returns>
        InterceptorProviderPolicy Build();

        /// <summary>
        /// Builds the specifc target type specific interception policy.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="configure">The delegate to configure specified target type based interception policy.</param>
        /// <returns>The current <see cref="IInterceptorProviderPolicyBuilder"/></returns>
        IInterceptorProviderPolicyBuilder To<TTarget>(Action<ITargetPolicyBuilder<TTarget>> configure);
    }
}
