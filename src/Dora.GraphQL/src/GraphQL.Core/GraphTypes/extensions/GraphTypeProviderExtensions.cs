using Dora.GraphQL.GraphTypes;
using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Defines <see cref="IGraphTypeProvider"/> specific extension methods.
    /// </summary>
    public static class GraphTypeProviderExtensions
    {
        /// <summary>
        /// Gets <see cref="IGraphType"/> based on specified name.
        /// </summary>
        /// <param name="provider">The <see cref="IGraphTypeProvider"/>.</param>
        /// <param name="graphTypeName">Name of the graph type.</param>
        /// <returns>The <see cref="IGraphType"/>.</returns>
        public static IGraphType GetGraphType(this IGraphTypeProvider provider, string graphTypeName)
        {
            Guard.ArgumentNotNull(provider, nameof(provider));
            Guard.ArgumentNotNullOrWhiteSpace(graphTypeName, nameof(graphTypeName));
            return provider.TryGetGraphType(graphTypeName, out var value)
                ? value
                : throw new ArgumentException($"Cannot resolve specified GraphType '{graphTypeName}'", nameof(graphTypeName));
        }
    }
}
