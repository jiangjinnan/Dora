using System;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents the generator to generate the interceptable dynamic proxy.
    /// </summary>
    public interface IDynamicProxyGenerator
    {  
        /// <summary>
        /// Determines whether this specified type can be intercepted.
        /// </summary>
        /// <param name="type">The type to intercept.</param>
        /// <returns>
        ///   <c>true</c> if the specified type can be intercepted; otherwise, <c>false</c>.
        /// </returns>
        bool CanIntercept(Type type);
    }
}
