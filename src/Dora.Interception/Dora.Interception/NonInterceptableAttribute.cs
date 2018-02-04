using System;

namespace Dora.Interception
{
    /// <summary>
    /// An attribute indicating the target method is not allowed to be intercepted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class NonInterceptableAttribute : Attribute
    {
        /// <summary>
        /// The type of interceptor provider to suppress.
        /// </summary>
        public Type[] InterceptorProviderTypes { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="NonInterceptableAttribute"/> class.
        /// </summary>
        /// <param name="interceptorProviderTypes">The interceptor provider types.</param>
        public NonInterceptableAttribute(params Type[] interceptorProviderTypes)
        {
            this.InterceptorProviderTypes = interceptorProviderTypes;
        }
    }
}
