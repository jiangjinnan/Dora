using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents the interface which the interceptable service descriptor must implement.
    /// </summary>
    public interface IInterceptableServiceDescriptor
    {
        /// <summary>
        /// The original target implementation type.
        /// </summary>
        Type TargetType { get; }
    }
}
