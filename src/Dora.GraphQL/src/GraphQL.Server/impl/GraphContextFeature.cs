using Dora.GraphQL.Executors;
using System;

namespace Dora.GraphQL.Server
{
    public class GraphContextFeature : IGraphContextFeature
    {
        public GraphContextFeature(GraphContext graphContext)
        {
            GraphContext = graphContext ?? throw new ArgumentNullException(nameof(graphContext));
        }
        public GraphContext GraphContext { get; }
    }
}
