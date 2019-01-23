using Dora.GraphQL.ArgumentBinders;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class ServiceProviderBinder : IArgumentBinder
    {
        public ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            var serviceProvider = context.ResolverContext.GraphContext.RequestServices;
            var type = context.Parameter.ParameterInfo.ParameterType;
            var argumentValue = serviceProvider.GetService(type);
            if (null != argumentValue)
            {
                return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Success(argumentValue));
            }
            return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Failed());
        }
    }
}
