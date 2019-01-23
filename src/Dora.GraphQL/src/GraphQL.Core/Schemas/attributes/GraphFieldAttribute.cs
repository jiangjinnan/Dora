using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Attribtue used to define GraphQL type's field.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property)]
    public sealed class GraphFieldAttribute: Attribute
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether target property should be ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        public bool Ignored { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if field is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the resolver method name.
        /// </summary>
        /// <value>
        /// The  resolver method name.
        /// </value>
        public string Resolver { get; set; }
    }
}
