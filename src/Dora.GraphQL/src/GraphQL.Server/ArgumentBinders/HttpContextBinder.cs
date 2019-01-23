using Dora.GraphQL.ArgumentBinders;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class HttpContextBinder : IArgumentBinder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextBinder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            if (context.Parameter.ParameterInfo.ParameterType == typeof(HttpContext))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Success(httpContext));
            }
            return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Failed());
        }
    }
}
