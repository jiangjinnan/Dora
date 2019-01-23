using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Attribues used to define known types.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Interface| AttributeTargets.Method, AllowMultiple = true)]
    public class KnownTypesAttribute: Attribute
    {
        /// <summary>
        /// Gets the known types.
        /// </summary>
        /// <value>
        /// The known types.
        /// </value>
        public Type[] Types { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnownTypesAttribute"/> class.
        /// </summary>
        /// <param name="types">The known types.</param>
        public KnownTypesAttribute(params Type[] types)
        {
            Guard.ArgumentNotNullOrEmpty(types, nameof(types));
            if (types.Any(it => it.IsEnumerable(out _)))
            {
                throw new ArgumentException($"Known type should not be created on enumerable types.");
            }
            Types = types;
        }
    }
}
