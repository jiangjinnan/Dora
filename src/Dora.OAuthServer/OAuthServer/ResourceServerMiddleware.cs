using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the resource server in OAuth2 model.
    /// </summary>
    public class ResourceServerMiddleware
    {
        private readonly RequestDelegate _next;
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the o authentication context factory.
        /// </summary>
        /// <value>
        /// The o authentication context factory.
        /// </value>
        public IOAuthContextFactory OAuthContextFactory { get => _serviceProvider.GetRequiredService<IOAuthContextFactory>(); }

        /// <summary>
        /// Gets the o authentication context validator.
        /// </summary>
        /// <value>
        /// The o authentication context validator.
        /// </value>
        public IOAuthContextValidator OAuthContextValidator { get => _serviceProvider.GetRequiredService<IOAuthContextValidator>(); }

        /// <summary>
        /// The related configuration options.
        /// </summary>
        public ResourceServerOptions Options { get; }

        /// <summary>
        /// Creates a new <see cref="ResourceServerMiddleware"/>.
        /// </summary>
        /// <param name="next">A<see cref="RequestDelegate"/> used to invoke the next middleware.</param>
        /// <param name="secureDataFormat">The <see cref="ISecureDataFormat{OAuthTicket}"/> to protect the authorization code, access token and refresh token.</param>
        /// <param name="systemClock">The <see cref="ISystemClock"/> to provide standard time.</param>
        /// <param name="accessTokenExtractor">The <see cref="IAccessTokenAccessor"/> to extract the access token.</param>
        /// <param name="accessTokenValidator">The <see cref="IAccessTokenValidator"/> to valide the access token.</param>
        /// <param name="optionsAccessor">A <see cref="IOptions{ResourceServerOptions}"/> to access the confiuration options.</param>
        public ResourceServerMiddleware(RequestDelegate next, IOptions<OAuthServerOptions> optionsAccessor)
        {
            _next = Guard.ArgumentNotNull(next, nameof(next));
            Options = optionsAccessor.Value.ResourceServer;
        }

        /// <summary>
        /// Handle the http request.
        /// </summary>
        /// <param name="context">The current request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to handle the request.</returns>
        public async Task Invoke(HttpContext context)
        {
           _serviceProvider = Guard.ArgumentNotNull(context, nameof(context)).RequestServices;
            var endpoint = Options.Endpoints.FirstOrDefault(it => it.Match(context));
            if (null == endpoint)
            {
                await _next(context);
                return;
            }

            var resourceContext = await OAuthContextFactory.CreateResourceContextAsync(context, endpoint);
            if (resourceContext.IsFaulted)
            {
                await SendErrorAsync(context, resourceContext.Error);
                return;
            }
           
            await OAuthContextValidator.ValidateResourceContextAsync(resourceContext);
            if (resourceContext.IsFaulted)
            {
                await SendErrorAsync(context, resourceContext.Error);
                return;
            }            
            await endpoint.Handler(resourceContext);
        }

        /// <summary>
        /// Respone error to the client.
        /// </summary>
        /// <param name="context">The current request specific <see cref="HttpContext"/>.</param>
        /// <param name="error">The <see cref="ResponseError"/> representing the error responsed to client.</param>
        /// <returns>The task to send error to the client.</returns>
        protected virtual async Task SendErrorAsync(HttpContext context, ResponseError error)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(error, nameof(error));

            context.Response.StatusCode = error.StatusCode;
            context.Response.ContentType = "application/json";
            var data = new
            {
                error = error.Error,
                error_description = error.Description
            };
            await context.Response.WriteAsync(JsonConvert.SerializeObject(data));
        }
    }
}