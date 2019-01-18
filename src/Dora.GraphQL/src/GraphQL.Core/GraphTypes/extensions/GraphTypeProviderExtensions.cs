using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{
    public static class GraphTypeProviderExtensions
    {
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
