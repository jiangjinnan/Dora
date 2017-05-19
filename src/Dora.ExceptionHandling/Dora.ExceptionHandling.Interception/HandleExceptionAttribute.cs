using Dora.Interception;
using System.Linq;

namespace Dora.ExceptionHandling.Interception
{
    /// <summary>
    /// The <see cref="ExceptionHandlingInterceptor"/> specific interceptor provider atrribute.
    /// </summary>
    public class HandleExceptionAttribute : InterceptorAttribute
    {
        /// <summary>
        /// The name of exception policy to handle the thrown exception.
        /// </summary>
        public string ExceptionPolicyName { get; }

        /// <summary>
        /// Create a <see cref="HandleExceptionAttribute"/>.
        /// </summary>
        /// <param name="exceptionPolicyName">The name of exception policy to handle the thrown exception.</param>
        /// <remarks>If the <paramref name="exceptionPolicyName"/> is not explicitly specified, the exception policy name specified by the <see cref="ExceptionPolicyAttribute"/> will be used.</remarks>
        public HandleExceptionAttribute(string exceptionPolicyName = null)
        {
            this.ExceptionPolicyName = exceptionPolicyName;
        }

        /// <summary>
        /// Register the provided interceptor to the specified interceptor chain builder.
        /// </summary>
        /// <param name="builder">The interceptor chain builder to which the provided interceptor is registered.</param>
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<ExceptionHandlingInterceptor>(this.Order, this.GetExceptionPolicyName());
        }

        /// <summary>
        /// Get the name of exception policy to handle the thrown exception.
        /// </summary>
        /// <returns>The name of exception policy to handle the thrown exception.</returns>
        protected virtual string GetExceptionPolicyName()
        {
            if (!string.IsNullOrWhiteSpace(this.ExceptionPolicyName))
            {
                return this.ExceptionPolicyName;
            }
            return this.Attributes.OfType<ExceptionPolicyAttribute>().FirstOrDefault()?.PolicyName;
        }
    }
}
