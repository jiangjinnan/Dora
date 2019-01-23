using System;
using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represent a GraphQL type
    /// </summary>
    public interface IGraphType
    {
        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        /// <value>
        /// The CLR type.
        /// </value>
        Type Type { get; }

        /// <summary>
        /// Gets the other types for union type.
        /// </summary>
        /// <value>
        /// The other types for union type.
        /// </value>
        /// <remarks>It is an empty array for non union type.</remarks>
        Type[] OtherTypes { get; }

        /// <summary>
        /// Gets the GraphQL type name.
        /// </summary>
        /// <value>
        /// The  GraphQL type name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this is a required (non-optional) GraphQL type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this is required; otherwise, <c>false</c>.
        /// </value>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a value indicating whether this is an enumerable (array) GraphQL type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this is enumerable; otherwise, <c>false</c>.
        /// </value>
        bool IsEnumerable { get; }

        /// <summary>
        /// Gets a value indicating whether this is an enum GraphQL type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this is enum; otherwise, <c>false</c>.
        /// </value>
        bool IsEnum { get; }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        /// <remarks>The key is field name + container CLR type. For union GraphQL types, all member types' fields are included.</remarks>
        IDictionary<NamedType, GraphField> Fields { get; }

        /// <summary>
        /// Resolves the value based on provided raw value.
        /// </summary>
        /// <param name="rawValue">The raw value.</param>
        /// <returns>The real value.</returns>
        object Resolve(object rawValue);
    }
}
