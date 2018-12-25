using Microsoft.AspNetCore.Http;
using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the context specific to authorization endpoint or token endpoint.
    /// </summary>
    public abstract class OAuthContext
    {
        #region Properties
        /// <summary>
        /// The current <see cref="HttpContext"/>.
        /// </summary>
        public HttpContext HttpContext { get; }        

        /// <summary>
        /// The error happen to process the OAuth request.
        /// </summary>
        public ResponseError Error { get; set; }

        /// <summary>
        /// Indicates whether the current request is authenticated or anonymous.
        /// </summary>
        public virtual bool IsAuthenticated
        {
            get { return HttpContext?.User?.Identity?.IsAuthenticated == true; }
        }

        /// <summary>
        /// Indicates whether the current request's method is GET.
        /// </summary>
        public bool IsGetRequest
        {
            get { return string.Equals(HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Indicates whether the current request's method is POST.
        /// </summary>
        public bool IsPostRequest
        {
            get { return string.Equals(HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Indicate whether the validation error has happed.
        /// </summary>
        public bool IsFaulted
        {
            get { return Error != null; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="OAuthContext"/>.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="httpContext"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="clientId"/> is a white space string.</exception>
        public OAuthContext(HttpContext httpContext)
        {
            HttpContext = Guard.ArgumentNotNull(httpContext, nameof(httpContext));
        }

        internal OAuthContext(HttpContext httpContext, ResponseError error)
        {
            HttpContext = httpContext;
            Error = error;
        }
        #endregion         
    }
}
