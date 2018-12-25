using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The middleware acted as the OAuth 2.0 authorization endpoint and token endpoint.
    /// </summary>
    public class AuthorizationServerMiddleware
    {
        #region Fields
        private readonly RequestDelegate _next;
        private IServiceProvider _serviceProvider;
        #endregion

        #region Properties
        /// <summary>
        /// The authorization server options.
        /// </summary>
        public AuthorizationServerOptions Options { get; }

        /// <summary>
        /// The OAuth grant store storing current authorization code, refresh token and access token.
        /// </summary>
        public IOAuthGrantStore OAuthGrantStore { get => _serviceProvider.GetRequiredService<IOAuthGrantStore>(); }

        /// <summary>
        /// The factory to create request contexts for authorization endpoint and token endpoint.
        /// </summary>
        public IOAuthContextFactory ContextFactory { get => _serviceProvider.GetRequiredService<IOAuthContextFactory>(); }

        /// <summary>
        /// The validator to validate the request contexts for authorization endpoint and token endpoint.
        /// </summary>
        public IOAuthContextValidator ContextValidator { get => _serviceProvider.GetRequiredService<IOAuthContextValidator>(); }

        /// <summary>
        /// The secure data format to encrypt and decrypt the OAuth ticket.
        /// </summary>
        public ISecureDataFormat<OAuthTicket> OAuthTicketFormat { get => _serviceProvider.GetRequiredService<ISecureDataFormat<OAuthTicket>>(); }

        /// <summary>
        /// The <see cref="UrlEncoder"/> to perform URL based encoding.
        /// </summary>
        public UrlEncoder UrlEncoder { get => _serviceProvider.GetRequiredService<UrlEncoder>(); }

        /// <summary>
        /// The system clock to provide uniform time for expiration checking.
        /// </summary>
        public ISystemClock SystemClock { get => _serviceProvider.GetRequiredService<ISystemClock>(); }

        /// <summary>
        /// The generator to generate fingerprint for authorization code, refresh token and access token.
        /// </summary>
        public ITokenValueGenerator  TokenValueGenerator { get => _serviceProvider.GetRequiredService<ITokenValueGenerator>(); }

        /// <summary>
        /// The <see cref="IDelegateScopeStore"/> to get all resource access delegate scopes.
        /// </summary>
        public IDelegateScopeStore DelegateScopeStore { get => _serviceProvider.GetRequiredService<IDelegateScopeStore>(); }

        /// <summary>
        /// The <see cref="IDelegateScopeStore"/> to load scope based delegate consent.
        /// </summary>
        public IDelegateConsentStore DelegateConsentStore { get => _serviceProvider.GetRequiredService<IDelegateConsentStore>(); }

        /// <summary>
        /// The logger used for error and audit logging.
        /// </summary>
        public ILogger Logger { get; }
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new <see cref="AuthorizationServerMiddleware"/>.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> used to invoke the next middleware.</param>
        /// <param name="optionsAccessor">The <see cref="IOptions{AuthorizationServerOptions}"/> used to get authorization server options.</param>
        /// <param name="contextFactory">The factory to create request contexts for authorization endpoint and token endpoint.</param>
        /// <param name="contextValidator">The validator to validate the request contexts for authorization endpoint and token endpoint.</param>
        /// <param name="oauthTicketFormat">The secure data format to encrypt and decrypt the OAuth ticket.</param>
        /// <param name="urlEncoder">The <see cref="UrlEncoder"/> to perform URL based encoding.</param>
        /// <param name="systemClock">The system clock to provide uniform time for expiration checking.</param>
        /// <param name="ticketFingerprintGenerator">The generator to generate fingerprint for authorization code, refresh token and access token.</param>
        /// <param name="authenticator">The <see cref="IAuthenticator"/> to perform authentication and sign-in.</param>
        /// <param name="delegateConsentConfirmCollector">The <see cref="IDelegateConsentConfirmCollector"/> to collect the user's resoruce access delegate decision.</param>
        /// <param name="logger">The <see cref="ILoggerFactory"/> to create logger.</param>
        /// <param name="oAuthGrantStore"> The <see cref="IOAuthGrantStore"/>The OAuth grant store storing current authorization code, refresh token and access token.</param>
        /// <param name="authenticateLogStore"></param>
        /// <param name="delegateScopeStore"></param>
        /// <param name="delegateConsentStore"></param>
        public AuthorizationServerMiddleware(
            RequestDelegate next,
            IOptions<OAuthServerOptions> optionsAccessor,
            ILogger<AuthorizationServerMiddleware> logger)
        {
            _next = Guard.ArgumentNotNull(next, nameof(next));
            Options = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.AuthorizationServer;           
            Logger = Guard.ArgumentNotNull(logger, nameof(logger));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Handle the authorization endpoint and token endpoint request.
        /// </summary>
        /// <param name="httpContext">The current request context specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to handle the authorization endpoint and token endpoint request.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            _serviceProvider =  Guard.ArgumentNotNull(httpContext, nameof(httpContext)).RequestServices;
            if (httpContext.Request.Path == Options.AuthorizationEndpoint)
            {
                var authorizationContext = await ContextFactory.CreateAuthorizationContextAsync(httpContext);
                try
                {
                    await HandleAuthorizationEndpointRequestAsync(httpContext, authorizationContext);
                }
                catch (Exception ex)
                {
                    await LogErrorAsync(ex);
                    authorizationContext.Failed(OAuthErrors.ServerError.ServerError);
                    await HandleErrorAsync(authorizationContext);
                }
                return;
            }

            if (httpContext.Request.Path == Options.TokenEndpoint)
            {
                var tokenContext = await ContextFactory.CreateTokenGrantContextAsync(httpContext);
                try
                {
                    await HandleTokenEndpointRequestAsync(httpContext, tokenContext);
                }
                catch (Exception ex)
                {
                    await LogErrorAsync(ex);
                    tokenContext.Failed(OAuthErrors.ServerError.ServerError);
                    await HandleErrorAsync(tokenContext);
                }
                return;
            }

            if (httpContext.Request.Path == Options.SignOutEndpoint)
            {
                await HandleSignOutEndpointRequestAsync(httpContext);
                return;
            }
            await _next(httpContext);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handle authorization endpoint specifc request.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="context">The <see cref="AuthorizationContext"/> representing the authorization endpoint based request context.</param>
        /// <returns>The task to handle authorization endpoint specifc request.</returns>
        protected internal virtual async Task HandleAuthorizationEndpointRequestAsync(HttpContext httpContext, AuthorizationContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            //Check whether context is valid, and continue if valid.
            if (await HandleErrorAsync(context))
            {
                return;
            }

            //Redirect to sign-in-url if not authenticated.
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                httpContext.Response.Redirect($"{Options.SignInUrl}?ReturnUrl={httpContext.GetRequestUrl()}&{httpContext.Request.QueryString.ToString().Substring(1)}");
                return;
            }

            //Validate AuthorizationContext
            await ContextValidator.ValidateAuthorizationContextAsync(context);
            if (await HandleErrorAsync(context))
            {
                return;
            }

            //If any delegate scope is not consented, redirect to delegate-consent-url
            var delegateConsent = await DelegateConsentStore.GetAsync(context.ClientId, context.HttpContext.User.Identity.Name);
            var scopesToConsent = context.Scopes.Except(delegateConsent?.Scopes ?? new string[0]).ToArray();
            if (scopesToConsent.Any())
            {
                httpContext.Response.Redirect($"{Options.DelegateConsentUrl}?ReturnUrl={httpContext.GetRequestUrl()}&{httpContext.Request.QueryString.ToString().Substring(1)}&Scopes={string.Join(";", scopesToConsent)}");
                return;               
            }          

            //Grant authorization code or access token
            if (context.ResponseType == OAuthResponseType.AuthorizationCode)
            {
                await GrantAuthorizationCodeAsync(httpContext, context);
            }
            else
            {
                await GrantAccessTokenAsync(context);
            }
        }

        /// <summary>
        /// Handle token endpoint specifc request.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="context">The <see cref="TokenContext"/> representing the token endpoint based request context.</param>
        /// <returns>The task to handle token endpoint specifc request.</returns>
        protected internal virtual async Task HandleTokenEndpointRequestAsync(HttpContext httpContext, TokenContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            //Check whether context is valid, and continue if valid.
            if (await HandleErrorAsync(context))
            {
                return;
            }

            //Validate TokenGrantContext
            await ContextValidator.ValidateTokenGrantContextAsync(context);
            if (await HandleErrorAsync(context))
            {
                return;
            }

            //Grant access token
            var authorizationCode = context.AuthorizationCode;
            string refreshTokenFingerprint = TokenValueGenerator.GenerateRefreshToken();
            string accessTokenFingerprint = TokenValueGenerator.GenerateAccessToken();
            string userName = await OAuthGrantStore.GetUserNameAsync(authorizationCode.Fingerprint);
            await OAuthGrantStore.UpdateTokensAsync(context.ClientId, userName, refreshTokenFingerprint, accessTokenFingerprint);

            var refeshToken = new OAuthTicket(userName, refreshTokenFingerprint, SystemClock.UtcNow.Add(Options.RefreshTokenLifetime), authorizationCode.Scopes);
            var accessToken = new OAuthTicket(userName, accessTokenFingerprint, SystemClock.UtcNow.Add(Options.AccessTokenLifetime), authorizationCode.Scopes);
            var token = new
            {
                access_token = OAuthTicketFormat.Protect(accessToken),
                token_type = Options.TokenType,
                expires_in = Options.AccessTokenLifetime.TotalSeconds,
                refresh_token = OAuthTicketFormat.Protect(refeshToken),
                scope = string.Join(" ", accessToken.Scopes)
            };
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(token), Encoding.UTF8);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private async Task HandleSignOutEndpointRequestAsync(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                await httpContext.SignOutAsync(Options.SignInScheme);
            }
            string returnUrl = httpContext.Request.Query.TryGetValue("returnurl", out StringValues values) ? values.ToString() : "/";
            httpContext.Response.Redirect(returnUrl);
        }

        /// <summary>
        /// Handle exception and response error information.
        /// </summary>
        /// <param name="context">A <see cref="OAuthContext"/> representing the authorization endpoint or token endpoint request context.</param>
        /// <returns>Indicates whether to stop.</returns>
        protected virtual async Task<bool> HandleErrorAsync(OAuthContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (context.Error == null)
            {
                return false;
            }
            await LogErrorAsync(context);

            //Authorization endpoint.
            AuthorizationContext authoriationContext = context as AuthorizationContext;
            if (null != authoriationContext && authoriationContext.RedirectUri != null)
            {
                string arguments = $"error={UrlEncoder.Encode(context.Error.Error)}&error_description={UrlEncoder.Encode(context.Error.Description)}";
                string seperator = authoriationContext.ResponseType == OAuthResponseType.AuthorizationCode ? "?" : "#";
                context.HttpContext.Response.Redirect($"{authoriationContext.RedirectUri}{seperator}{arguments}");
                return true;
            }

            //Token endpoint
            TokenContext tokenGrantContext = context as TokenContext;
            if (null != tokenGrantContext)
            {
                context.HttpContext.Response.StatusCode = context.Error.StatusCode;
                context.HttpContext.Response.ContentType = "application/json";
                var error = new
                {
                    error = context.Error.Error,
                    error_description = context.Error.Description
                };
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(error), Encoding.UTF8);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Issue the authorization code (Authorization Code Grant).
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="context">The <see cref="AuthorizationContext"/> representing the authorization endpoint based request context.</param>
        /// <returns>The task to issue the authorization code.</returns>
        protected virtual async Task GrantAuthorizationCodeAsync(HttpContext httpContext, AuthorizationContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            var userName = context.HttpContext.User.Identity.Name;

            var fingerprint = TokenValueGenerator.GenerateAuthorizationCode();
            var authorizationCode = new OAuthTicket(userName, fingerprint, SystemClock.UtcNow.Add(Options.AuthorizationCodeLifetime), context.Scopes);
            await OAuthGrantStore.UpdateAuthorizationCodeAsync(context.ClientId, userName, fingerprint);
            var url = $"{context.RedirectUri}?code={OAuthTicketFormat.Protect(authorizationCode)}";
            url += $"&state={context.State}";
            string[] keys = new string[] { "state", "response_type", "client_id", "redirect_uri", "scope" };
            foreach (var q in context.HttpContext.Request.Query)
            {
                if (!keys.Contains(q.Key, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (string value in q.Value)
                    {
                        url += $"&{q.Key}={UrlEncoder.Encode(value)}";
                    }
                }
            }
            context.HttpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Issue the access token (Implicit Grant).
        /// </summary>
        /// <param name="context">The <see cref="AuthorizationContext"/> representing the authorization endpoint based request context.</param>
        /// <returns>The task to issue the access token.</returns>
        protected virtual async Task GrantAccessTokenAsync(AuthorizationContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            var userName = context.HttpContext.User.Identity.Name;
            var fingerprint = TokenValueGenerator.GenerateAccessToken();
            var accessToken = new OAuthTicket(userName, fingerprint, SystemClock.UtcNow.Add(Options.AccessTokenLifetime), context.Scopes);
            await OAuthGrantStore.UpdateTokensAsync(context.ClientId, userName, null, fingerprint);
            var url = $"{context.RedirectUri}#access_token={UrlEncoder.Encode(OAuthTicketFormat.Protect(accessToken))}";
            url += $"&token_type={Options.TokenType}";
            url += $"&expires_in={Options.AccessTokenLifetime.TotalSeconds}";
            url += $"&scope={string.Join(" ", accessToken.Scopes)}";
            url += $"&state={UrlEncoder.Encode(context.State)}";
            string[] keys = new string[] { "state", "response_type", "client_id", "redirect_uri", "scope" };
            foreach (var q in context.HttpContext.Request.Query)
            {
                if (!keys.Contains(q.Key, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (string value in q.Value)
                    {
                        url += $"&{q.Key}={UrlEncoder.Encode(value)}";
                    }
                }
            }
            context.HttpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Perform error logging.
        /// </summary>
        /// <param name="context">A <see cref="OAuthContext"/> representing the authorization endpoint or token endpoint request context.</param>
        /// <returns>The task to perform error logging.</returns>
        protected virtual Task LogErrorAsync(OAuthContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (context.IsFaulted && context.Error.Error != OAuthErrors.ServerError.ServerError.Category)
            {
                return Task.Run(() => Logger.LogError(context.Error.Description));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Perform error logging.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> thrown during processing request.</param>
        /// <returns>The task to perform error logging.</returns>
        protected virtual Task LogErrorAsync(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(exception.Message);
            builder.AppendLine($"Type: {exception.GetType().AssemblyQualifiedName}");
            builder.AppendLine($"StackTrace: {exception.StackTrace}");
            Logger.LogError(builder.ToString());
            return Task.CompletedTask;
        }
        #endregion
    }
}