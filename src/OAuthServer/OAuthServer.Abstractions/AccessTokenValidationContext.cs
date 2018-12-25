using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The execution context to validate the access token.
    /// </summary>
    public class AccessTokenValidationContext
    {
        /// <summary>
        /// The current request specific <see cref="HttpContext"/>.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// The validation error.
        /// </summary>
        public ResponseError Error { get; set; }

        /// <summary>
        /// The access token to validate.
        /// </summary>
        public OAuthTicket AccessToken { get; set; }

        /// <summary>
        /// The scopes for target resource endpoint.
        /// </summary>
        public IEnumerable<string> TargetScopes { get; }

        /// <summary>
        /// Creates a new <see cref="AccessTokenValidationContext"/>.
        /// </summary>
        /// <param name="httpContext">The current request specific <see cref="HttpContext"/>.</param>
        /// <param name="accessToken">The validation error.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="httpContext"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="accessToken"/> is null.</exception>
        public AccessTokenValidationContext(HttpContext httpContext, OAuthTicket accessToken)
        {
            HttpContext = Guard.ArgumentNotNull(httpContext, nameof(httpContext));
            AccessToken = Guard.ArgumentNotNull(accessToken, nameof(accessToken));
            TargetScopes = new string[0];
        }

        /// <summary>
        /// Creates a new <see cref="AccessTokenValidationContext"/>.
        /// </summary>
        /// <param name="httpContext">The current request specific <see cref="HttpContext"/>.</param>
        /// <param name="accessToken">The validation error.</param>
        /// <param name="targetScopes">The scopes for target resource endpoint.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="httpContext"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="accessToken"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="targetScopes"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="targetScopes"/> is empty or it has white space element.</exception>
        public AccessTokenValidationContext(HttpContext httpContext, OAuthTicket accessToken, IEnumerable<string> targetScopes)
        {
            HttpContext = Guard.ArgumentNotNull(httpContext, nameof(httpContext));
            AccessToken = Guard.ArgumentNotNull(accessToken, nameof(accessToken));
            TargetScopes = Guard.ElementNotNullOrWhiteSpace(targetScopes, nameof(targetScopes), false);
        }

        /// <summary>
        /// Indicate whether the validation error has happed.
        /// </summary>
        public bool IsFaulted
        {
            get { return Error != null; }
        }
    }
}
