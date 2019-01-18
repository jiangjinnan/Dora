using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Extract the access token from current request.
    /// </summary>
    public interface IAccessTokenAccessor
    {
        /// <summary>
        /// Try to extract the protected access token from current request.
        /// </summary>
        /// <param name="context">The current request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to extract the protected access token from current request.</returns>
        Task<string> GetAccessTokenAsync(HttpContext context);
    }
}
