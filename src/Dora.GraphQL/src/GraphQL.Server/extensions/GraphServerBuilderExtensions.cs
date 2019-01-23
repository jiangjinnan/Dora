using Dora.GraphQL;
using Dora.GraphQL.Options;
using Dora.GraphQL.Server;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GraphServerBuilderExtensions
    {
        public static GraphServerBuilder UsePascalCase(this GraphServerBuilder builder)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.ConfigureGraphQL(options => options.FieldNamingConvention = FieldNamingConvention.PascalCase);
        }

        public static GraphServerBuilder UseCamelCase(this GraphServerBuilder builder)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.ConfigureGraphQL(options => options.FieldNamingConvention = FieldNamingConvention.CamelCase);
        }

        public static GraphServerBuilder UsePathBase(this GraphServerBuilder builder, PathString pathBase)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.ConfigureServer(options => options.PathBase = pathBase);
        }
    }
}
