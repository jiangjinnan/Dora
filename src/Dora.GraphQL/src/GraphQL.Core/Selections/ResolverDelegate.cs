using Dora.GraphQL.GraphTypes;
using System.Threading.Tasks;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents GraphQL resolver specific delegate.
    /// </summary>
    /// <param name="resolverContext">The <see cref="ResolverContext"/>.</param>
    /// <returns>The resolved value.</returns>
    public delegate ValueTask<object>  ResolverDelegate(ResolverContext resolverContext);
}
