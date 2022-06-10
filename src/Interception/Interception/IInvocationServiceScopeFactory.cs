using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    /// <summary>
    /// Factory to create method invocation based service scope.
    /// </summary>
    public interface IInvocationServiceScopeFactory
    {
        /// <summary>
        /// Creates method invocation based service scope.
        /// </summary>
        /// <returns>The created method invocation based service scope.</returns>
        IServiceScope CreateInvocationScope();
    }
}
