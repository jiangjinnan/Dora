using Dora.OAuthServer;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOAuthServer(this IApplicationBuilder app)
        {
            Guard.ArgumentNotNull(app, nameof(app));
            return app
                .UseMiddleware<AuthorizationServerMiddleware>()
                .UseMiddleware<ResourceServerMiddleware>();
        }
    }
}
