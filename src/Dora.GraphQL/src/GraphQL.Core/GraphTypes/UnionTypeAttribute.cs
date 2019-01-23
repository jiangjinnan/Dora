using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Attribute used to define union type members.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class UnionTypeAttribute: Attribute
    {
        /// <summary>
        /// Gets the all union type members.
        /// </summary>
        /// <value>
        /// All union type members..
        /// </value>
        public Type[] Types { get; }

        /// <summary>
        /// Gets the first union type member.
        /// </summary>
        /// <value>
        /// The first union type member..
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets the other union type member.
        /// </summary>
        /// <value>
        /// The other union type member.
        /// </value>
        public Type[] OtherTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionTypeAttribute"/> class.
        /// </summary>
        /// <param name="types">The union type member.</param>
        public UnionTypeAttribute(params Type[] types)
        {
            if (types.Length <= 1)
            {
                throw new ArgumentException($"Union GraphType should be created on at least 2 sclar types.",nameof(types));
            }

            if (types.Any(it => it.IsEnumerable(out _)))
            {
                throw new ArgumentException($"Union GraphType can not be created on enumerable types.");
            }
            Type = types[0];
            var otherTypes = new Type[types.Length - 1];
            for (int index = 1; index < types.Length; index++)
            {
                otherTypes[index - 1] = types[index];
            }
            OtherTypes = otherTypes;
            Types = types;
        }
    }
}
