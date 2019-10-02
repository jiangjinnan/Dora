using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Represents an interception operation.
    /// </summary>
    /// <param name="context">The context for the invocation to the proxy.</param>
    /// <returns>The task to perform interception operation.</returns>
    public delegate Task InterceptDelegate(InvocationContext context);
}
