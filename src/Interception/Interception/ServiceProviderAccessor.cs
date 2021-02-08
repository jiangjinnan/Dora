using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public class ServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        }

        public IServiceProvider ServiceProvider => _httpContextAccessor?.HttpContext?.RequestServices ?? _serviceProvider;
    }
}
