using Dora.GraphQL;
using Dora.GraphQL.Server;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraphQLServer(this IApplicationBuilder app)
        {
            Guard.ArgumentNotNull(app, nameof(app));           
            return app.UseMiddleware<GraphQLServerMiddleware>();
        }
    }
}
