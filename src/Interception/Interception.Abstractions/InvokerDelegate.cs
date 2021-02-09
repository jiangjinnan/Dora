using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Interceptor chain invoker.
    /// </summary>
    /// <param name="invocationContext">The invocation context.</param>
    /// <returns>The task to invoke the interceptor chain.</returns>
    public delegate Task InvokerDelegate(InvocationContext invocationContext);
}
