using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// A factory to create request cotnext for authorization endpoint and token endpoiint.
    /// </summary>
    public interface IOAuthContextFactory
    {
        /// <summary>
        /// Create authoriztion endpoint specific request context.
        /// </summary>
        /// <param name="httpContext">The current HTTP request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to create the authoriztion endpoint specific request context.</returns>
        Task<AuthorizationContext> CreateAuthorizationContextAsync(HttpContext httpContext);

        /// <summary>
        /// Create token endpoint specific request context.
        /// </summary>
        /// <param name="httpContext">The current HTTP request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to create the token endpoint specific request context.</returns>
        Task<TokenContext> CreateTokenGrantContextAsync(HttpContext httpContext);

        /// <summary>
        /// Creates resource accessing based context.
        /// </summary>
        /// <param name="httpContext">The current HTTP request specific <see cref="HttpContext" />.</param>
        /// <param name="resourceEndpoint">The resource endpoint.</param>
        /// <returns>
        /// The task to create resource accessing based context.
        /// </returns>
        Task<ResourceContext> CreateResourceContextAsync(HttpContext httpContext, ResourceEndpoint resourceEndpoint);
    }
}
