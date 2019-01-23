using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents GraphQL directive.
    /// </summary>
    public interface IDirective
    {
        /// <summary>
        /// Gets the directive name.
        /// </summary>
        /// <value>
        /// The directive name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        IDictionary<string, NamedValueToken> Arguments { get; }
    }
}
