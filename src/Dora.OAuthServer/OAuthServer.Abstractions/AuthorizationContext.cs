using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The authorization endpoint specific request context.
    /// </summary>
    public class AuthorizationContext : OAuthContext
    {
        /// <summary>
        /// The client identifier of the application.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The redirect uri of the application.
        /// </summary>
        public Uri RedirectUri { get; set; }

        /// <summary>
        /// The response type (AuthorizationCode or AccessToken)
        /// </summary>
        public OAuthResponseType ResponseType { get; }

        /// <summary>
        /// The OAuth scopes.
        /// </summary>
        public IEnumerable<string> Scopes { get; }

        /// <summary>
        /// The state passed from client to prevent cross-site request forgery
        /// </summary>
        public string State { get; }

        /// <summary>
        /// Creates a new <see cref="AuthorizationContext"/>.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <param name="clientId">The client identifier of the application.</param>
        /// <param name="redirectUri">The redirect uri of the application.</param>
        /// <param name="responseType">The response type (AuthorizationCode or AccessToken)</param>
        /// <param name="scope">The OAuth scopes.</param>
        /// <param name="state">The state passed from client to prevent cross-site request forgery.</param>
        public AuthorizationContext(HttpContext httpContext, string clientId, Uri redirectUri, OAuthResponseType responseType, IEnumerable<string> scope, string state) 
            : base(httpContext) 
        {
            ClientId = Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            RedirectUri = Guard.ArgumentNotNull(redirectUri, nameof(redirectUri));
            ResponseType = responseType;
            Scopes = scope ?? new string[0];
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationContext"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="error">The error.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        public AuthorizationContext(HttpContext httpContext, ResponseError error, Uri redirectUri = null) : base(httpContext, error)
        {
            RedirectUri = redirectUri;
        }
    }
}