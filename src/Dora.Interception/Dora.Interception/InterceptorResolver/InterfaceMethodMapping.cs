using System;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Retrieves the mapping of an interface into the actual methods on a class that implements that interface.
    /// </summary>
    public struct InterfaceMethodMapping
    {
        /// <summary>
        /// The methods that are defined on the interface.
        /// </summary>
        public MethodInfo[] InterfaceMethods;

        /// <summary>
        /// The type that was used to create the interface mapping.
        /// </summary>
        public Type TargetType;

        /// <summary>
        /// Shows the type that represents the interface.
        /// </summary>
        public Type InterfaceType;

        /// <summary>
        /// The methods that implement the interface.
        /// </summary>
        public MethodInfo[] TargetMethods;

        /// <summary>
        /// Create a new <see cref="InterfaceMethodMapping"/> based specified <see cref="InterfaceMapping"/>.
        /// </summary>
        /// <param name="mapping">The <see cref="InterfaceMapping"/>.</param>
        public InterfaceMethodMapping(InterfaceMapping mapping)
        {
            InterfaceMethods = mapping.InterfaceMethods;
            TargetMethods = mapping.TargetMethods;
            InterfaceType = mapping.InterfaceType;
            TargetType = mapping.TargetType;
        }
    }
}