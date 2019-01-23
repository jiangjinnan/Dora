using Dora.GraphQL.GraphTypes;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    internal class FakeResolver : IGraphResolver
    {
        public static readonly FakeResolver Instance = new FakeResolver();
        public ValueTask<object> ResolveAsync(ResolverContext context) => default;
        private FakeResolver() { }
    }
}
