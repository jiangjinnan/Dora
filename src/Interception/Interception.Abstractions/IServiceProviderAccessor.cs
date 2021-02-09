using System;

namespace Dora.Interception
{
    /// <summary>
    /// Accessor to get <see cref="IServiceProvider"/> for interceptor activation.
    /// </summary>
    public interface IServiceProviderAccessor
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> for interceptor activation.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceProvider"/> for interceptor activation.
        /// </value>
        IServiceProvider ServiceProvider { get; }
    }
}
