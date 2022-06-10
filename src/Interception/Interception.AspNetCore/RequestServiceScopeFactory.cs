using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Flight.Interception.AspNetCore
{
    internal class RequestServiceScopeFactory :  IInvocationServiceScopeFactory
    {
        private readonly InvocationServiceScopeFactory _factory;
        private readonly IHttpContextAccessor  _httpContextAccessor;
        private NullServiceScope _nullServiceScope;

        public RequestServiceScopeFactory(IServiceProvider  serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _factory = ActivatorUtilities.CreateInstance<InvocationServiceScopeFactory>(serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public IServiceScope CreateInvocationScope()
        {
            _nullServiceScope ??= new NullServiceScope (_httpContextAccessor);
            return _httpContextAccessor.HttpContext == null? _factory.CreateInvocationScope(): _nullServiceScope;
        }

        private class NullServiceScope : IServiceScope
        {
            private readonly IHttpContextAccessor _httpContextAccessor;
            public NullServiceScope(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;
            public IServiceProvider ServiceProvider => _httpContextAccessor.HttpContext?.RequestServices!;
            public void Dispose() { }
        }
    }
}
