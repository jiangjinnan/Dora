using System.Threading.Tasks;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphResolver
    {
        ValueTask<object> ResolveAsync(ResolverContext context);
    }
}
