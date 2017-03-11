using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Represents interceptor provider.
    /// </summary>
    public interface IInterceptorProvider
    {
        /// <summary>
        /// Register the provided interceptor to the specified interceptor chain builder.
        /// </summary>
        /// <param name="builder">The interceptor chain builder to which the provided interceptor is registered.</param>
        void Use(IInterceptorChainBuilder builder);

        /// <summary>
        /// Indicate whether multiple interceptors of the same type can be applied to a single one method.
        /// </summary>
        bool AllowMultiple { get; }
    }
}
