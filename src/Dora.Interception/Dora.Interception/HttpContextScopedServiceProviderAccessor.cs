using Microsoft.AspNetCore.Http;
using System;

namespace Dora.Interception
{
    public class HttpContextScopedServiceProviderAccessor : IScopedServiceProviderAccesssor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextScopedServiceProviderAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public IServiceProvider Current => _httpContextAccessor?.HttpContext?.RequestServices;
    }
}
