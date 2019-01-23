using Dora.GraphQL.Descriptors;
using Dora.GraphQL.Schemas;
using System;

namespace Dora.GraphQL.Server
{
    public class GraphSchemaProvider : IGraphSchemaProvider
    {
        private readonly Lazy<IGraphSchema> _schemaAccessor;

        public GraphSchemaProvider(ISchemaFactory schemaFactory, IGraphServiceDiscoverer serviceDiscoverer)
        {
            Guard.ArgumentNotNull(schemaFactory, nameof(schemaFactory));
            Guard.ArgumentNotNull(serviceDiscoverer, nameof(serviceDiscoverer));

            _schemaAccessor = new Lazy<IGraphSchema>(() =>
            {               
                return schemaFactory.Create(serviceDiscoverer.Services);
            });
        }

        public IGraphSchema Schema { get => _schemaAccessor.Value; }
    }
}
