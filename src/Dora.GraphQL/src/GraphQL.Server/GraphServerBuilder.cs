using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.GraphQL.Server
{
    public class GraphServerBuilder
    {
        public IServiceCollection Services { get; }

        public GraphServerBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public GraphServerBuilder ConfigureGraphQL(Action<GraphOptions> configure)
        {
            Services.Configure(Guard.ArgumentNotNull(configure,  nameof(configure)));
            return this;
        }

        public GraphServerBuilder ConfigureServer(Action<GraphServerOptions> configure)
        {
            Services.Configure(Guard.ArgumentNotNull(configure, nameof(configure)));
            return this;
        }
    }
}
