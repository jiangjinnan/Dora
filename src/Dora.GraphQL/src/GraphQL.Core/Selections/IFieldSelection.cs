using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents field specific <see cref="ISelectionNode"/>.
    /// </summary>
    public interface IFieldSelection :ISelectionNode
    {
        /// <summary>
        /// Gets the field name.
        /// </summary>
        /// <value>
        /// The field name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        string Alias { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        IDictionary<string, NamedValueToken> Arguments { get; }

        /// <summary>
        /// Gets the directives.
        /// </summary>
        /// <value>
        /// The directives.
        /// </value>
        ICollection<IDirective> Directives { get; }
    }
}