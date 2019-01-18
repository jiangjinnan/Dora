using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The bearer based access token extractor.
    /// </summary>
    public class AccessTokenAccessor : IAccessTokenAccessor
    {
        /// <summary>
        /// Try to extract the protected access token from current request.
        /// </summary>
        /// <param name="context">The current request specific <see cref="HttpContext"/>.</param>
        /// <returns>The task to extract the protected access token from current request.</returns>
        public Task<string> GetAccessTokenAsync(HttpContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (!context.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues values))
            {
                return Task.FromResult<string>(null);
            }
            foreach (string it in values)
            {
                if (it.StartsWith("Bearer "))
                {
                    return Task.FromResult(it.Split(' ')[1]);
                }
            }
            return Task.FromResult<string>(null);
        }
    }
}
