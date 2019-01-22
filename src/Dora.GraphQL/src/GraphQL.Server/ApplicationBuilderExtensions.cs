using Dora.GraphQL;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Options;
using Dora.GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGraphQLServer(this IApplicationBuilder app)
        {
            Guard.ArgumentNotNull(app, nameof(app));
            var namingConvention = app.ApplicationServices.GetRequiredService<IOptions<GraphServerOptions>>().Value.FieldNamingConvention;
            GraphValueResolver.JsonSerializer = namingConvention == FieldNamingConvention.PascalCase
                ? new JsonSerializer()
                : new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return app.UseMiddleware<GraphQLServerMiddleware>();
        }
    }
}
