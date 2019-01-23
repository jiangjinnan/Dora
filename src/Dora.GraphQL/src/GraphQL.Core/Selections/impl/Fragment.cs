using Dora.GraphQL.GraphTypes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// The default implementation of <see cref="IFragment"/>.
    /// </summary>
    public class Fragment : IFragment
    {
        /// <summary>
        /// Gets fragement specific <see cref="IGraphType" />.
        /// </summary>
        /// <value>
        /// The fragement specific <see cref="IGraphType" />
        /// </value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the sub selection set.
        /// </summary>
        /// <value>
        /// The sub selection set.
        /// </value>
        public ICollection< ISelectionNode> SelectionSet { get; }

        /// <summary>
        /// Gets the custom property dictionary.
        /// </summary>
        /// <value>
        /// The custom property dictionary.
        /// </value>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fragment"/> class.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        public Fragment(IGraphType graphType)
        {
            GraphType = Guard.ArgumentNotNull( graphType, nameof(graphType));
            SelectionSet = new Collection<ISelectionNode>();
            Properties = new Dictionary<string, object>();
        }
    }
}
