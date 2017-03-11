using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// An attribute based interceptor provider.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method,AllowMultiple = false)]
    public abstract class InterceptorAttribute : Attribute, IInterceptorProvider
    {
        private bool? _allowMultiple;

        /// <summary>
        /// The order for the registered interceptor in the interceptor chain.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Indicate whether multiple interceptors of the same type can be applied to a single one method.
        /// </summary>
        public bool AllowMultiple
        {
           get
            {
                return _allowMultiple.HasValue
                   ? _allowMultiple.Value
                   : (_allowMultiple = this.GetType().GetTypeInfo().GetCustomAttribute<AttributeUsageAttribute>().AllowMultiple).Value;
            }
        }

        /// <summary>
        /// Register the provided interceptor to the specified interceptor chain builder.
        /// </summary>
        /// <param name="builder">The interceptor chain builder to which the provided interceptor is registered.</param>
        public abstract void Use(IInterceptorChainBuilder builder);
    }
}