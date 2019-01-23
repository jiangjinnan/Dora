using System.Threading.Tasks;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represent a resolver to generate each node's value.
    /// </summary>
    public interface IGraphResolver
    {
        /// <summary>
        /// Resolves the value the current selection node.
        /// </summary>
        /// <param name="context">The <see cref="ResolverContext"/> in which the field value is resoved.</param>
        /// <returns>The resolved field's value.</returns>
        ValueTask<object> ResolveAsync(ResolverContext context);
    }
}
