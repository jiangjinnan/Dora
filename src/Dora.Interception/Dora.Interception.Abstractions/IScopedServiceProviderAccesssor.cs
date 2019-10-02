using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    /// <summary>
    /// Represents accessor to get current ambient scoped service provider.
    /// </summary>
    public interface IScopedServiceProviderAccesssor
    {
        /// <summary>
        /// Gets current ambient scoped service provider.
        /// </summary>
        IServiceProvider Current { get; } 
    }
}
