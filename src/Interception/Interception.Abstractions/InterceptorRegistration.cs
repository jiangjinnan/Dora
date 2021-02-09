using System;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Interceptor registration.
    /// </summary>
    public sealed class InterceptorRegistration
    {
        /// <summary>
        /// Gets the interceptor factory.
        /// </summary>
        /// <value>
        /// The interceptor factory.
        /// </value>
        public Func<IServiceProvider, object> InterceptorFactory { get; }

        /// <summary>
        /// Gets the target method the interceptor is assigned to.
        /// </summary>
        /// <value>
        /// The target method the interceptor is assigned to.
        /// </value>
        public MethodInfo TargetMethod { get; }

        /// <summary>
        /// Gets the order representing the interceptor's position in the interceptor chain.
        /// </summary>
        /// <value>
        /// The order representing the interceptor's position in the interceptor chain.
        /// </value>
        public int Order { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorRegistration"/> class.
        /// </summary>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        /// <param name="targetMethod">The target method the interceptor is assigned to.</param>
        /// <param name="order">The order representing the interceptor's position in the interceptor chain.</param>
        public InterceptorRegistration(Func<IServiceProvider, object> interceptorFactory, MethodInfo targetMethod, int order)
        {
            InterceptorFactory = interceptorFactory ?? throw new ArgumentNullException(nameof(interceptorFactory));
            TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            Order = order;
        }
    }
}