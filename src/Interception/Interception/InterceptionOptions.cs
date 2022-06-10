using Dora.Interception.Expressions;

namespace Dora.Interception
{
    /// <summary>
    /// Interception based options.
    /// </summary>
    public class InterceptionOptions
    {
        /// <summary>
        /// Gets or sets the interceptor registrations.
        /// </summary>
        /// <value>
        /// The interceptor registrations.
        /// </value>
        public Action<IInterceptorRegistry>? InterceptorRegistrations { get; set; } 
    }
}
