using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Dora.Interception
{
    /// <summary>
    /// Annotate this attribute to interceptable service classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InterceptableServiceAttribute: Attribute
    {
        /// <summary>
        /// Gets the service types.
        /// </summary>
        /// <value>
        /// The service types.
        /// </value>
        public Type[] ServiceTypes { get; }

        /// <summary>
        /// Gets the lifetime.
        /// </summary>
        /// <value>
        /// The lifetime.
        /// </value>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptableServiceAttribute"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime.</param>
        /// <param name="mapTo">The map to.</param>
        public InterceptableServiceAttribute(ServiceLifetime lifetime, params Type[] mapTo)
        {
            ServiceTypes = mapTo.Any()? mapTo : Type.EmptyTypes;
            Lifetime = lifetime;
        }
    }
}
