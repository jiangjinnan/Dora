using Dora.GraphQL.Descriptors;
using System.Collections.Generic;

namespace Dora.GraphQL.Schemas
{
    /// <summary>
    /// Represents GraphQL schema factory.
    /// </summary>
    public interface ISchemaFactory
    {
        /// <summary>
        /// Creates the GraphQL schema factory based on specified GraphQL services.
        /// </summary>
        /// <param name="services">The <see cref="GraphServiceDescriptor"/> list.</param>
        /// <returns>The <see cref="IGraphSchema"/>.</returns>
        IGraphSchema Create(IEnumerable<GraphServiceDescriptor> services);
    }
}
