using System.Collections.Generic;

namespace Dora.GraphQL.Descriptors
{
    /// <summary>
    /// Represents a GraphQL serivce discoverer.
    /// </summary>
    public interface IGraphServiceDiscoverer
    {
        /// <summary>
        /// Gets all GraphQL services.
        /// </summary>
        /// <value>
        /// The <see cref="GraphServiceDescriptor"/> list.
        /// </value>
        IEnumerable<GraphServiceDescriptor> Services { get; }
    }
}
