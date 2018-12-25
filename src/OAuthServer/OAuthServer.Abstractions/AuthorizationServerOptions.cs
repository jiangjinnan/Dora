using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Authorization service based options.
    /// </summary>
    public class AuthorizationServerOptions
    {
        /// <summary>
        /// The path of authorization endpoint.
        /// </summary>
        public string AuthorizationEndpoint { get; set; }

        /// <summary>
        /// The path of token endpoint.
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// sign out EndPoint
        /// </summary>
        public string SignOutEndpoint { get; set; }

        /// <summary>
        /// The URL of the login page.
        /// </summary>
        public string SignInUrl { get; set; }

        /// <summary>
        /// The URL of the delegate consent page used by the resource owner to make reosurce access delegate decision. 
        /// </summary>
        public string DelegateConsentUrl { get; set; }

        /// <summary>
        /// Expiration time of authorization code.
        /// </summary>
        public TimeSpan AuthorizationCodeLifetime { get; set; }

        /// <summary>
        /// Expiration time of access token.
        /// </summary>
        public TimeSpan AccessTokenLifetime { get; set; }

        /// <summary>
        /// Expiration refresh token.
        /// </summary>
        public TimeSpan RefreshTokenLifetime { get; set; }

        /// <summary>
        /// The authentication scheme for authentication ticket.
        /// </summary>
        public string SignInScheme { get; set; }

        /// <summary>
        /// The type of access token type.
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// The scope based options.
        /// </summary>
        public IList<ScopeOptions> Scopes { get; set; }

        public IEnumerable<string> DefaultScopes { get => Scopes.Where(it => !it.Optional).Select(it => it.Id).ToArray(); }

        /// <summary>
        /// Creates a new <see cref="AuthorizationServerOptions"/>.
        /// </summary>
        public AuthorizationServerOptions()
        {
            AuthorizationEndpoint = AuthorizationServerOptionDefaults.AuthorizationEndpoint;
            TokenEndpoint = AuthorizationServerOptionDefaults.TokenEndpoint;
            SignOutEndpoint = AuthorizationServerOptionDefaults.SignOutEndpoint;
            SignInUrl = AuthorizationServerOptionDefaults.LoginPageUrl;
            DelegateConsentUrl = AuthorizationServerOptionDefaults.DelegateConsentPageUrl;

            AuthorizationCodeLifetime = AuthorizationServerOptionDefaults.AuthorizationCodeExpiration;
            AccessTokenLifetime = AuthorizationServerOptionDefaults.AccessTokenExpiration;
            RefreshTokenLifetime = AuthorizationServerOptionDefaults.RefreshTokenExpiration;

            TokenType = AuthorizationServerOptionDefaults.TokenType;
            Scopes = new List<ScopeOptions>();
        }
    }
}