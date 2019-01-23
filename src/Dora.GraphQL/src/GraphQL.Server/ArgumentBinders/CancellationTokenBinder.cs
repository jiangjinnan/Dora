using Dora.GraphQL.ArgumentBinders;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class CancellationTokenBinder: IArgumentBinder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CancellationTokenBinder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            if (context.Parameter.ParameterInfo.ParameterType == typeof(CancellationToken))
            {
                var requestAborted = _httpContextAccessor.HttpContext.RequestAborted;
                return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Success(requestAborted));
            }
            return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Failed());
        }
    }
}
