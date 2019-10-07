using Microsoft.AspNetCore.Http;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a custom <see cref="IScopedServiceProviderAccesssor"/> leverages <see cref="IHttpContextAccessor"/> to get current ambient <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class HttpContextScopedServiceProviderAccessor : IScopedServiceProviderAccesssor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Create a new <see cref="HttpContextScopedServiceProviderAccessor"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> used to get current <see cref="HttpContext"/>.</param>
        public HttpContextScopedServiceProviderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        /// <summary>
        /// Gets current ambient scoped service provider.
        /// </summary>
        public IServiceProvider Current => _httpContextAccessor?.HttpContext?.RequestServices;
    }
}
