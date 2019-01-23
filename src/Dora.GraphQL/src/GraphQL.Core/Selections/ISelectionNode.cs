using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents GraphQL query selection node.
    /// </summary>
    public interface ISelectionNode
    {
        /// <summary>
        /// Gets the sub selection set.
        /// </summary>
        /// <value>
        /// The sub selection set.
        /// </value>
        ICollection<ISelectionNode> SelectionSet { get; }

        /// <summary>
        /// Gets the custom property dictionary.
        /// </summary>
        /// <value>
        /// The custom property dictionary.
        /// </value>
        IDictionary<string, object> Properties { get; }
    }
}