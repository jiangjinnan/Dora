using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    public class OAuthContextValidator : IOAuthContextValidator
    {
        #region Properties
        /// <summary>
        /// The <see cref="IApplicationStore"/> to get registered applications.
        /// </summary>
        public IApplicationStore ApplicationStore { get; }

        /// <summary>
        /// The <see cref="IResourceAccessDelegateScopeStore"/> is get resource access delegate scope.
        /// </summary>
        public IDelegateScopeStore ScopeStore { get; }

        /// <summary>
        /// The <see cref="IOAuthGrantStore"/> to get currently effective authorization code, access token and refresh token.
        /// </summary>
        public IOAuthGrantStore OAuthGrantStore { get; }

        /// <summary>
        /// The <see cref="ISystemClock"/> to provide standard time.
        /// </summary>
        public ISystemClock SystemClock { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="OAuthRequestContextValidator"/>.
        /// </summary>
        /// <param name="appStore">The <see cref="IApplicationStore"/> to get registered applications.</param>
        /// <param name="scopeStore">The <see cref="IResourceAccessDelegateScopeStore"/> is get resource access delegate scope.</param>
        /// <param name="oauthGrantStore">The <see cref="IOAuthGrantStore"/> to get currently effective authorization code, access token and refresh token.</param>
        /// <param name="systemClock">The <see cref="ISystemClock"/> to provide standard time.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="appStore"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="scopeStore"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="oauthGrantStore"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="systemClock"/> is null.</exception>
        public OAuthContextValidator(IApplicationStore appStore, IDelegateScopeStore scopeStore, IOAuthGrantStore oauthGrantStore, ISystemClock systemClock)
        {
            ApplicationStore = Guard.ArgumentNotNull(appStore, nameof(appStore));
            ScopeStore = Guard.ArgumentNotNull(scopeStore, nameof(scopeStore));
            OAuthGrantStore = Guard.ArgumentNotNull(oauthGrantStore, nameof(oauthGrantStore));
            SystemClock = Guard.ArgumentNotNull(systemClock, nameof(systemClock));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validate the authorization endpoint specific context.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <returns>The task to validate the authorization endpoint specific context.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        public async Task ValidateAuthorizationContextAsync(AuthorizationContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            //1. Check if the context has errors
            if (context.IsFaulted)
            {
                return;
            }

            //2. Check if the request is authenticated
            if (!context.IsAuthenticated)
            {
                throw new InvalidOperationException("Request is not authenticated.");
            }

            //3. Check if provided client Id is valid.
            var application = await ValidateClientIdAsync(context);
            if (context.IsFaulted)
            {
                return;
            }

            //4. Check if redirect_uri is valid.
            await ValidateRedirectUriAsync(context, application);
            if (context.IsFaulted)
            {
                return;
            }

            //5. Check if provide scopes are all authorized.
            await ValidateScopesAsync(context);
        }

        /// <summary>
        /// Validate the token endpoint specific context.
        /// </summary>
        /// <param name="context">The acess token endpoint specific context.</param>
        /// <returns>The task to validate the token endpoint specific context.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        public async Task ValidateTokenGrantContextAsync(TokenContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            //1. Check if provided client Id is valid.
            var application = await ValidateClientIdAsync(context);
            if (context.IsFaulted)
            {
                return;
            }

            //3. Check if client_secret is valid.
            await ValidateClientSecret(context, application);
            if (context.IsFaulted)
            {
                return;
            }

            //3. Check if redirect_uri is valid.
            await ValidateRedirectUriAsync(context, application);
            if (context.IsFaulted)
            {
                return;
            }

            //4. Validate Authorization Code or Refresh Token
            if (context.GrantType == OAuthGrantType.AuthorizationCode)
            {
                await ValidateAuthorizationCodeAsync((TokenContext)context);
            }
            else
            {
                await ValidateRefreshTokenAsync((TokenContext)context);
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Check if the specified client ID is valid.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <returns>A <see cref="Task{Application}"/> whose result represents the specified client ID specific application. </returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        protected virtual async Task<Application> ValidateClientIdAsync(OAuthContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            var clientId = ((context as AuthorizationContext)?.ClientId)
                ?? (context as TokenContext)?.ClientId;

            if (string.IsNullOrEmpty(clientId))
            {
                return null;
            }
            var application = await ApplicationStore.GetByClientIdAsync(clientId);
            if (null == application)
            {
                context.Failed(OAuthErrors.UnauthorizedClient.UnregisteredApplication, clientId);
            }
            return application;
        }

        /// <summary>
        /// Check whether the specified resource access delegate scope(s) is (are) valid.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <returns>The task to check whether the specified resource access delegate scope(s) is (are) valid.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        protected virtual async Task ValidateScopesAsync(AuthorizationContext context)
        {
            var validScopes = (await ScopeStore.GetAllAsync()).Select(it => it.Id);
            var except = context.Scopes.Except(validScopes);
            if (except.Any())
            {
                context.Failed(OAuthErrors.InvalidScope.InvalidScope, except.First());
                return;
            }
        }

        /// <summary>
        /// Check whether the specified redirect URI is valid.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <param name="application">The given client ID specific <see cref="Application"/>.</param>
        /// <returns>The task to check whether the specified redirect URI is valid.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="application"/> is null.</exception>
        protected virtual Task ValidateRedirectUriAsync(OAuthContext context, Application application)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(application, nameof(application));

            var authorizationContext = context as AuthorizationContext;
            var tokenContext = context as TokenContext;
            if (authorizationContext == null && tokenContext == null)
            {
                return Task.CompletedTask;
            }
            
            var redirectUri = (context as AuthorizationContext)?.RedirectUri
                ?? (context as TokenContext).RedirectUri;

            //'redirect_uri' must be absolute.
            if (redirectUri != null && !redirectUri.IsAbsoluteUri)
            {
                context.Failed(OAuthErrors.InvalidRequest.InvalidRedirectUri);
                return Task.CompletedTask;
            }

            //If registered redirect_uri >1, must explicitly provided.
            if (redirectUri == null && application.RedirectUris.Count > 1)
            {
                context.Failed(OAuthErrors.InvalidRequest.MissingRedirectUri);
                return Task.CompletedTask;
            }
            //'redirect_uri' must match the registered one.
            if (redirectUri != null && !application.RedirectUris.Any(it => it.Equals(redirectUri)))
            {
                context.Failed(OAuthErrors.InvalidGrant.InvalidRedirectUri);
                return Task.CompletedTask;
            }

            //Use the registered redirect_uri
            if (null != authorizationContext)
            {
                authorizationContext.RedirectUri = authorizationContext.RedirectUri ?? application.RedirectUris.Single();
            }
            else
            {
                tokenContext.RedirectUri = tokenContext.RedirectUri ?? application.RedirectUris.Single();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check whether the specified authorization code is valid.
        /// </summary>
        /// <param name="context">The acess token endpoint specific context.</param>
        /// <returns>The task to check whether the specified authorization code is valid.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        protected virtual async Task ValidateAuthorizationCodeAsync(TokenContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (context.AuthorizationCode.HasExpired(SystemClock))
            {
                context.Failed(OAuthErrors.InvalidGrant.AuthorizationCodeHasExpired);
            }
            if (!await OAuthGrantStore.VaidateAuthorizationCodeAsync(context.AuthorizationCode.Fingerprint))
            {
                context.Failed(OAuthErrors.InvalidGrant.AuthorizationCodeIsRevoked);
            }
        }

        /// <summary>
        /// Check whether the specified refresh token is valid.
        /// </summary>
        /// <param name="context">The acess token endpoint specific context.</param>
        /// <returns>The task to check whether the specified authorization code is valid.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        protected virtual async Task ValidateRefreshTokenAsync(TokenContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (context.RefreshToken.HasExpired(SystemClock))
            {
                context.Failed(OAuthErrors.InvalidGrant.RefreshTokenHasExpired);
            }
            if (!await OAuthGrantStore.VaidateRefreshTokenAsync(context.RefreshToken.Fingerprint))
            {
                context.Failed(OAuthErrors.InvalidGrant.RefreshTokenIsRevoked);
            }
        }

        /// <summary>
        /// Check whether the specified client secret is valid.
        /// </summary>
        /// <param name="context">The acess token endpoint specific context.</param>
        /// <param name="application">The given client ID specific <see cref="Application"/>.</param>
        /// <returns>The task to check whether the specified authorization code is valid.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="application"/> is null.</exception>
        protected virtual Task ValidateClientSecret(TokenContext context, Application application)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(application, nameof(application));
            if (context.ClientSecret != application.ClientSecret)
            {
                context.Failed(OAuthErrors.InvalidGrant.InvalidClientSecret);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates resource accessing context.
        /// </summary>
        /// <param name="context">The resource accessing context.</param>
        /// <returns>
        /// The task to validate the resource accessing context.
        /// </returns>
        public async Task ValidateResourceContextAsync(ResourceContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            if (context.OAuthTicket.HasExpired(SystemClock))
            {
                context.Failed(OAuthErrors.UnauthorizedClient.AccessTokenHasExpired);
                return;
            }

            if (!await OAuthGrantStore.VaidateAccessTokenAsync(context.OAuthTicket.Fingerprint))
            {
                context.Failed(OAuthErrors.UnauthorizedClient.AccessTokenIsRevoked);
                return;
            }

            if (!context.ResourceEndpoint.Scopes.Intersect(context.OAuthTicket.Scopes).Any())
            {
                context.Failed(OAuthErrors.UnauthorizedClient.UnauthorizedScopes);
            }
        }
        #endregion
    }
}
