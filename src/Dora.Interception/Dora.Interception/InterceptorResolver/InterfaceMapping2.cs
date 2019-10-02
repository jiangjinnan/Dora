using System;
using System.Reflection;

namespace Dora.Interception
{
    //
    // Summary:
    //     Retrieves the mapping of an interface into the actual methods on a class that
    //     implements that interface.
    public struct InterfaceMapping2
    {
        //
        // Summary:
        //     Shows the methods that are defined on the interface.
        public MethodInfo[] InterfaceMethods;
        //
        // Summary:
        //     Shows the type that represents the interface.
        public Type InterfaceType;
        //
        // Summary:
        //     Shows the methods that implement the interface.
        public MethodInfo[] TargetMethods;
        //
        // Summary:
        //     Represents the type that was used to create the interface mapping.
        public Type TargetType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        public InterfaceMapping2(InterfaceMapping mapping)
        {
            InterfaceMethods = mapping.InterfaceMethods;
            TargetMethods = mapping.TargetMethods;
            InterfaceType = mapping.InterfaceType;
            TargetType = mapping.TargetType;
        }
    }
}