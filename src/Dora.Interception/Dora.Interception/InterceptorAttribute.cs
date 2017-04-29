using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// An attribute based interceptor provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public abstract class InterceptorAttribute : Attribute, IInterceptorProvider, IAttributeCollection
    {
        private readonly ConcurrentBag<Attribute> _attributes = new ConcurrentBag<Attribute>();
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
        /// Gets all attributes applied to the target method and class.
        /// </summary>
        public IEnumerable<Attribute> Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Register the provided interceptor to the specified interceptor chain builder.
        /// </summary>
        /// <param name="builder">The interceptor chain builder to which the provided interceptor is registered.</param>
        public abstract void Use(IInterceptorChainBuilder builder);

        #region IAttributeCollection
        IAttributeCollection IAttributeCollection.Add(Attribute attribute)
        {
            Guard.ArgumentNotNull(attribute, nameof(attribute));
            _attributes.Add(attribute);
            return this;
        }

        IAttributeCollection IAttributeCollection.AddRange(IEnumerable<Attribute> attributes)
        {
            Guard.ArgumentNotNull(attributes, nameof(attributes));
            foreach (var attribute in attributes)
            {
                _attributes.Add(attribute);
            }
            return this;
        }

        IEnumerator<Attribute> IEnumerable<Attribute>.GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }
        #endregion
    }
}