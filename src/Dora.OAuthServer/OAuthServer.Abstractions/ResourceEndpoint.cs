using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represent an endpoint of resource server.
    /// </summary>
    public class ResourceEndpoint
    {
        /// <summary>
        /// The path of resource endpoint.
        /// </summary>
        public PathString Path { get;  }

        /// <summary>
        /// The scope of resource endpoint.
        /// </summary>
        public IEnumerable<string> Scopes { get; }

        /// <summary>
        /// The handler to handle the request.
        /// </summary>
        public ResourceHandlerDelegate Handler { get; }

        /// <summary>
        /// Suportted http methods.
        /// </summary>
        public IEnumerable<HttpMethod> AllowMethods { get; }

        /// <summary>
        /// Creates a new <see cref="ResourceEndpoint"/>.
        /// </summary>
        /// <param name="path">The path of resource endpoint.</param>
        /// <param name="allowMethods">Suportted http methods.</param>
        /// <param name="handler">The handler to handle the request.</param>
        /// <param name="scope">The first scope of resource endpoint.</param>
        /// <param name="otherScopes">The other scopes of resource endpoint.</param>
        public ResourceEndpoint(PathString path, IEnumerable<HttpMethod> allowMethods , ResourceHandlerDelegate handler, params string[] scopes)
        {
            Path = path;
            AllowMethods = Guard.ArgumentNotNullOrEmpty(allowMethods, nameof(allowMethods));
            Handler = Guard.ArgumentNotNull(handler, nameof(handler));
            Scopes = Guard.ArgumentNotNullOrEmpty(scopes, nameof(scopes));
        }

        /// <summary>
        /// Indicates if match the request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> representing the request to match.</param>
        /// <returns>Indicates whether to match the specified request.</returns>
        public virtual bool Match(HttpContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (!AllowMethods.Any(it => string.Equals(it.Method, context.Request.Method, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return context.Request.Path.Equals(Path);
        }
    }
}
