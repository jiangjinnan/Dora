using AutoMapper;
using Dora.GraphQL;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL.Execution;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQLServer(this IServiceCollection services, Action<GraphServerOptions> configure = null)
        {
            services.AddHttpContextAccessor();
            services.TryAddSingleton<ISchemaFactory, SchemaFactory>();
            services.TryAddSingleton<IAttributeAccessor, AttributeAccessor>();
            services.TryAddSingleton<IGraphTypeProvider, GraphTypeProvider>();
            services.TryAddSingleton<IGraphContextFactory, DefaultGraphContextFactory>();
            services.TryAddSingleton<IDocumentBuilder, GraphQLDocumentBuilder>();
            services.TryAddSingleton<IGraphSchemaProvider, GraphSchemaProvider>();
            services.TryAddSingleton<ISelectionSetProvider, SelectionSetProvider>();
            services.TryAddSingleton<IGraphExecutor, DefaultGraphExecutor>();
            services.TryAddSingleton<IQueryResultTypeGenerator, QueryResultTypeGenerator>();
            services.TryAddSingleton<IGraphSchemaFormatter, GraphSchemaFormatter>();

            if (configure != null)
            {
                services.Configure<GraphServerOptions>(configure);
            }

            Mapper.Initialize(cfg => cfg.CreateMissingTypeMaps = true);
            return services;
        }
    }
}
