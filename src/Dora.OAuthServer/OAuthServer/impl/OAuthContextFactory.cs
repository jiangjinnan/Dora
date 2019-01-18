using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    public  class OAuthContextFactory: IOAuthContextFactory
    {
        #region Fields
        private readonly string[] _validResponseTypes;
        private readonly IEnumerable<string> _defaultScope;
        private readonly IEnumerable<string> _valideGrantTypes;
        private readonly IAccessTokenAccessor _tokenAccessor;
        private readonly ISecureDataFormat<OAuthTicket> _ticketFormat;
        #endregion

        /// <summary>
        /// Creates a new <see cref="OAuthRequestContextFactory"/>.
        /// </summary>
        /// <param name="oauthTockeFormat">A <see cref="ISecureDataFormat{OAuthTicket} "/> used for OAuth ticket encryption.</param>
        /// <param name="defaultScope">The default resource access scopes.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="oauthTockeFormat"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="defaultScope"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="defaultScope"/> is empty.</exception>
        public OAuthContextFactory(
            ISecureDataFormat<OAuthTicket> ticketFormat,
            IAccessTokenAccessor tokenAccessor,
            IOptions<OAuthServerOptions> optionsAccessor)
        {
            _ticketFormat = Guard.ArgumentNotNull(ticketFormat, nameof(ticketFormat));
            _tokenAccessor = Guard.ArgumentNotNull(tokenAccessor, nameof(tokenAccessor));
            _validResponseTypes = new string[] { "code", "token" };
            _valideGrantTypes = new string[] { "authorization_code", "refresh_token" };
            _defaultScope = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.AuthorizationServer.DefaultScopes;
        }

        /// <summary>
        /// Create authoriztion endpoint specific request context.
        /// </summary>
        /// <param name="httpContext">The current HTTP request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to create the authoriztion endpoint specific request context.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="httpContext"/> is null.</exception>
        public Task<AuthorizationContext> CreateAuthorizationContextAsync(HttpContext httpContext)
        {
            Guard.ArgumentNotNull(httpContext, nameof(httpContext));
            var query = httpContext.Request.Query;

            //Extract redirect_uri
            var redirectUriString = query.GetValue(OAuthDefaults.ParameterNames.RedirectUri);
            Uri redirectUri;
            try
            {
                redirectUri = new Uri(redirectUriString);
            }
            catch
            {
                return Task.FromResult(new AuthorizationContext(httpContext, OAuthErrors.InvalidRequest.InvalidRedirectUri.Format()));
            }

            //Extract response_type
            var responseTypeString = query.GetValue(OAuthDefaults.ParameterNames.ResponseType);
            if (string.IsNullOrWhiteSpace(responseTypeString))
            {
                return Task.FromResult(new AuthorizationContext(httpContext, OAuthErrors.InvalidRequest.MissingResponseType.Format(), redirectUri));
            }

            //Validate response_type
            if (!_validResponseTypes.Contains(responseTypeString))
            {
                return Task.FromResult(new AuthorizationContext(httpContext, OAuthErrors.UnsupportedResponseType.UnsupportedResponseType.Format(responseTypeString), redirectUri));
            }

            OAuthResponseType responseType = responseTypeString == "code"
                ? OAuthResponseType.AuthorizationCode
                : OAuthResponseType.AccessToken;

            //Extract client_id 
            var clientId = query.GetValue(OAuthDefaults.ParameterNames.ClientId);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return Task.FromResult(new AuthorizationContext(httpContext, OAuthErrors.InvalidRequest.MissingClientId.Format(), redirectUri));
            }

            var state = query.GetValue(OAuthDefaults.ParameterNames.State);
            var scope = query.GetValue(OAuthDefaults.ParameterNames.Scope);
            if (!string.IsNullOrWhiteSpace(scope))
            {
                return Task.FromResult(new AuthorizationContext(httpContext, clientId, redirectUri, responseType, scope.Split(' '), state));
            }
            return Task.FromResult(new AuthorizationContext(httpContext, clientId, redirectUri, responseType, _defaultScope, state));
        }

        public async Task<ResourceContext> CreateResourceContextAsync(HttpContext httpContext, ResourceEndpoint resourceEndpoint)
        {
            Guard.ArgumentNotNull(httpContext, nameof(httpContext));
            Guard.ArgumentNotNull(resourceEndpoint, nameof(resourceEndpoint));

            string protectedAccessToken = await _tokenAccessor.GetAccessTokenAsync(httpContext);
            if (string.IsNullOrEmpty(protectedAccessToken))
            {
                return new ResourceContext(httpContext, OAuthErrors.UnauthorizedClient.MissingAccessToken.Format());
            }

            var ticket = _ticketFormat.Unprotect(protectedAccessToken);
            if (null == ticket)
            {
                return new ResourceContext(httpContext, OAuthErrors.InvalidGrant.InvalidAccessToken.Format());
            }

            return new ResourceContext(httpContext, ticket,resourceEndpoint);
        }

        /// <summary>
        /// Create token endpoint specific request context.
        /// </summary>
        /// <param name="httpContext">The current HTTP request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to create the token endpoint specific request context.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="httpContext"/> is null.</exception>
        public Task<TokenContext> CreateTokenGrantContextAsync(HttpContext httpContext)
        {
            Guard.ArgumentNotNull(httpContext, nameof(httpContext));

            //Must be POST request.
            if (!string.Equals(httpContext.Request.Method, "POST"))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.UnsupportedHttpMethod.Format()));
            }

            //Conent-Type = application/x-www-form-urlencoded.
            if (!httpContext.Request.HasFormContentType)
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.UnsupportedContentType.Format()));
            }

            var form = httpContext.Request.Form;

            //Extract client_id.
            var clientId = form.GetValue(OAuthDefaults.ParameterNames.ClientId);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingClientId.Format()));
            }

            //Extract redirect_uri
            var redirectUriString = form.GetValue(OAuthDefaults.ParameterNames.RedirectUri);
            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingRedirectUri.Format()));
            }
            Uri redirectUri;
            try
            {
                redirectUri = new Uri(redirectUriString);
            }
            catch
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.InvalidRedirectUri.Format()));
            }

            //Extract client_secret
            var clientSecret = form.GetValue(OAuthDefaults.ParameterNames.ClientSecret);
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingClientSecret.Format()));
            }

            //Extract grant_type
            var grantTypeString = form.GetValue(OAuthDefaults.ParameterNames.GarntType);
            if (string.IsNullOrWhiteSpace(grantTypeString))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingGrantType.Format()));
            }
            if (!_valideGrantTypes.Contains(grantTypeString))
            {
                return Task.FromResult(new TokenContext(httpContext, OAuthErrors.UnsupportedGrantType.UnsupportedGrantType.Format(grantTypeString)));
            }

            OAuthGrantType grantType = grantTypeString == "authorization_code"
               ? OAuthGrantType.AuthorizationCode
               : OAuthGrantType.RefreshToken;

            if (grantType == OAuthGrantType.AuthorizationCode)
            {
                var code = form.GetValue(OAuthDefaults.ParameterNames.Code);
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingAuthorizationCode.Format()));
                }

                try
                {
                    OAuthTicket authorizationCode = _ticketFormat.Unprotect(code);
                    return Task.FromResult(new TokenContext(httpContext, clientId, redirectUri, clientSecret, authorizationCode, grantType));
                }
                catch
                {
                    return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidGrant.InvalidAuthorizationCode.Format()));
                }
            }
            else
            {
                var token = form.GetValue(OAuthDefaults.ParameterNames.RefreshToken);
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidRequest.MissingRefreshToken.Format()));
                }
                try
                {
                    OAuthTicket refreshToken = _ticketFormat.Unprotect(token);
                    return Task.FromResult(new TokenContext(httpContext, clientId, redirectUri, clientSecret, refreshToken, grantType));
                }
                catch
                {
                    return Task.FromResult(new TokenContext(httpContext, OAuthErrors.InvalidGrant.InvalidRefreshToken.Format()));
                }
            }
        }
    }
}
