using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.Interception.Policies
{
    /// <summary>
    /// The <see cref="IInterceptorProvider"/> specific interception policy.
    /// </summary>
    public class InterceptorProviderPolicy
    {
        /// <summary>
        /// Gets the type of <see cref="IInterceptorProvider"/>.
        /// </summary>
        /// <value>
        /// The type of <see cref="IInterceptorProvider"/>.
        /// </value>
        public Type InterceptorProviderType { get; }

        /// <summary>
        /// Gets the factory to create <see cref="IInterceptorProvider"/>.
        /// </summary>
        /// <value>
        /// The factory to create <see cref="IInterceptorProvider"/>.
        /// </value>
        public Func<IInterceptorProvider> InterceptorProviderFactory { get; }

        /// <summary>
        /// Gets the target type based policies.
        /// </summary>
        /// <value>
        /// The target type based policies.
        /// </value>
        public IList<TargetTypePolicy> TargetPolicies { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorProviderPolicy"/> class.
        /// </summary>
        /// <param name="interceptorProviderType">Type of the interceptor provider.</param>
        /// <param name="interceptorProviderFactory">The interceptor provider factory.</param>
        /// <param name="targetRegistrations">The target registrations.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptorProviderType"/> is null.</exception>  
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptorProviderFactory"/> is null.</exception>   
        public InterceptorProviderPolicy( 
           Type interceptorProviderType,
           Func<IInterceptorProvider> interceptorProviderFactory,
           IEnumerable<TargetTypePolicy> targetRegistrations = null)
        {
            InterceptorProviderType = Guard.ArgumentNotNull(interceptorProviderType, nameof(interceptorProviderType));
            InterceptorProviderFactory = Guard.ArgumentNotNull(interceptorProviderFactory, nameof(interceptorProviderFactory));
            TargetPolicies = targetRegistrations?.ToList() ?? new List<TargetTypePolicy>();
        } 
    }
}
