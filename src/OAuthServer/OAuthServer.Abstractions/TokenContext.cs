using Microsoft.AspNetCore.Http;
using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the token endpoint specific request context.
    /// </summary>
    public class TokenContext : OAuthContext
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
        /// Client sesret of the application.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// Authorization code provided by client application.
        /// </summary>
        public OAuthTicket AuthorizationCode { get; }

        /// <summary>
        /// Refresh token provided by client application.
        /// </summary>
        public OAuthTicket RefreshToken { get; }

        /// <summary>
        /// The OAuth grant type.
        /// </summary>
        public OAuthGrantType GrantType { get; }

        /// <summary>
        /// Creates a new <see cref="TokenContext"/>.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <param name="clientId">The client identifier of the application.</param>
        /// <param name="redirectUri">The redirect uri of the application.</param>
        /// <param name="clientSecret">Client sesret of the application.</param>
        /// <param name="authorizationCodeOrRefreshToken">Authorization code or refresh token provided by client application.</param>
        /// <param name="grantType">The OAuth grant type.</param>
        public TokenContext(HttpContext httpContext, string clientId, Uri redirectUri, string clientSecret, OAuthTicket authorizationCodeOrRefreshToken, OAuthGrantType grantType) : base(httpContext)
        {
            ClientId = Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            RedirectUri = Guard.ArgumentNotNull(redirectUri, nameof(redirectUri));
            ClientSecret = Guard.ArgumentNotNullOrWhiteSpace(clientSecret, nameof(clientSecret));
            GrantType = grantType;
            if (grantType == OAuthGrantType.AuthorizationCode)
            {
                AuthorizationCode = Guard.ArgumentNotNull(authorizationCodeOrRefreshToken, nameof(authorizationCodeOrRefreshToken));
            }
            else
            {
                RefreshToken = Guard.ArgumentNotNull(authorizationCodeOrRefreshToken, nameof(authorizationCodeOrRefreshToken));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenContext"/> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="error">The error.</param>
        public TokenContext(HttpContext httpContext, ResponseError error) : base(httpContext, error)
        { }
    }
}