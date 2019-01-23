using AutoMapper;
using Dora.GraphQL;
using Dora.GraphQL.ArgumentBinders;
using Dora.GraphQL.Descriptors;
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
        public static IServiceCollection AddGraphQLServer(this IServiceCollection services, Action<GraphServerBuilder> configure = null)
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
            services.TryAddSingleton<IJsonSerializerProvider, JsonSerializerProvider>();
            services.TryAddSingleton<IGraphServiceDiscoverer, GraphServiceDiscoverer>();
            services.TryAddSingleton<IArgumentBinderProvider, ArgumentBinderProvider>();

            services.AddSingleton<IArgumentBinder, GraphArgumentBinder>();
            services.AddSingleton<IArgumentBinder, HttpContextBinder>();
            services.AddSingleton<IArgumentBinder, GraphContextBinder>();
            services.AddSingleton<IArgumentBinder, ServiceProviderBinder>();
            services.AddSingleton<IArgumentBinder, CancellationTokenBinder>();


            if (configure != null)
            {
                var builder = new GraphServerBuilder(services);
                configure(builder);
            }

            Mapper.Initialize(cfg => cfg.CreateMissingTypeMaps = true);
            return services;
        }
    }
}
