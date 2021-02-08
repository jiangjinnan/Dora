using Microsoft.AspNetCore.Http;
using System;

namespace Dora.Interception.AspNetCore
{
    public class RequestServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public RequestServiceProviderAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _httpContextAccessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        }

        public IServiceProvider ServiceProvider => _httpContextAccessor?.HttpContext?.RequestServices ?? _serviceProvider;
    }
}
