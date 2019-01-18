using Dora.OAuthServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OAuthServerBuilderExtensions
    {
        public static OAuthServerBuilder<TUser> Configure<TUser>(this OAuthServerBuilder<TUser> builder, Action<OAuthServerOptions> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configure, nameof(configure));
            builder.Services.Configure(configure);
            return builder;
        }

        public static OAuthServerBuilder AddScope(this OAuthServerBuilder builder,
            string id,
            string title,
            string description,
            string iconUrl,
            bool optional)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(id, nameof(id));
            Guard.ArgumentNotNullOrWhiteSpace(title, nameof(title));
            Guard.ArgumentNotNullOrWhiteSpace(description, nameof(description));
            builder.Services.Configure<OAuthServerOptions>(options => options.AuthorizationServer.Scopes.Add(new ScopeOptions
            {
                Id = id,
                Title = title,
                IconUrl = iconUrl,
                Description = description,
                Optional = optional
            }));
            return builder;
        }

        public static OAuthServerBuilder<TUser> AddScope<TUser>(this OAuthServerBuilder<TUser> builder,
           string id,
           string title,
           string description,
           string iconUrl,
           bool optional)
        {
            AddScope((OAuthServerBuilder)builder, id, title, description, iconUrl, optional);
            return builder;
        }

        public static OAuthServerBuilder<TUser> AddUserInfoEndpoint<TUser>(
            this OAuthServerBuilder<TUser> builder, 
            string path, 
            Func<ResourceContext, Task<TUser>> userAccessor, 
            params string[] scopes)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(path, nameof(path));
            Guard.ArgumentNotNull(userAccessor, nameof(userAccessor));
            Guard.ArgumentNotNullOrEmpty(scopes, nameof(scopes));

            async Task handler(ResourceContext context)
            {
                try
                {
                    TUser user = await userAccessor(context);
                    context.HttpContext.Response.ContentType = "application/json";
                    var settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<OAuthServerOptions>>().Value.ResourceServer.JsonSerializerSettings;
                    await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(user, settings));
                }
                catch
                {
                    context.HttpContext.Response.StatusCode = 500;
                }
            }
            return builder.Configure(options => options.ResourceServer.Endpoints.Add(new ResourceEndpoint(new PathString(path), new HttpMethod[] { new HttpMethod("POST"), new HttpMethod("GET") }, handler, scopes)));
        }
    }
}
