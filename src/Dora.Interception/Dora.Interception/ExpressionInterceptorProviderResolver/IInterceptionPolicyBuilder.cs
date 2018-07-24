using System;
using System.Collections.Generic;


namespace Dora.Interception
{
    /// <summary>
    /// Define method to build interception policy.
    /// </summary>
    public interface IInterceptionPolicyBuilder
    {
        /// <summary>
        /// Get a service provider to get dependency services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Builds the interception policy composed of a series of <see cref="InterceptorProviderRegistration"/>.
        /// </summary>
        /// <returns>A <see cref="InterceptorProviderRegistration"/> collection.</returns>
        IEnumerable<InterceptorProviderRegistration> Build();

        /// <summary>
        /// Build the interception policy specific to specified interceptor provider type.
        /// </summary>
        /// <typeparam name="TInterceptorProvider">The type of the interceptor provider.</typeparam>
        /// <param name="order">The position of the generated interceptor in interception chain.</param>
        /// <param name="configureTargets">The configure targets.</param>
        /// <param name="arguments">The arguments to create interceptor provider.</param>
        /// <returns>The current <see cref="IInterceptionPolicyBuilder"/>.</returns>
        IInterceptionPolicyBuilder For<TInterceptorProvider>(int order, Action<IInterceptorProviderRegistrationBuilder> configureTargets, params object[] arguments)
            where TInterceptorProvider: IInterceptorProvider;
    }
}
