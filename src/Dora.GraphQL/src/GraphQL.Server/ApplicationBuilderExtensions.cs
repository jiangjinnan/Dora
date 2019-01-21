using Dora.GraphQL;
using Dora.GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

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
