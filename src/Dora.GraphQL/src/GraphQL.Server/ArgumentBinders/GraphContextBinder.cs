using Dora.GraphQL.ArgumentBinders;
using Dora.GraphQL.Executors;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class GraphContextBinder : IArgumentBinder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GraphContextBinder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            if (context.Parameter.ParameterInfo.ParameterType == typeof(GraphContext))
            {
                var graphContext = _httpContextAccessor.HttpContext.Features.Get<IGraphContextFeature>().GraphContext;
                return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Success(graphContext));
            }
            return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Failed());
        }
    }
}
