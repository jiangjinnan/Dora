using Dora.GraphQL.GraphTypes;
using System.Threading.Tasks;

namespace Dora.GraphQL.Selections
{
    public delegate ValueTask<object>  ResolverDelegate(ResolverContext resolverContext);
}
