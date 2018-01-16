using System;
using System.Reflection;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents the invocation context specific to calling the proxy.
    /// </summary>
    public abstract class InvocationContext
    {
        /// <summary>
        ///  Gets the <see cref="MethodInfo"/> representing the method of type to intercept.
        /// </summary>
        public abstract MethodBase Method { get; }       

        /// <summary>
        /// Gets the proxy object on which the intercepted method is invoked.
        /// </summary>
        public abstract object Proxy { get; }

        /// <summary>
        /// Gets the object on which the invocation is performed.
        /// </summary>
        public abstract object Target { get; } 

        /// <summary>
        /// Gets the arguments that target method has been invoked with.
        /// </summary>
        /// <remarks>Each argument is writable.</remarks>
        public abstract object[] Arguments { get; }  

        /// <summary>
        /// Gets or sets the return value of the method.
        /// </summary>
        public abstract object ReturnValue { get; set; }  
    }
}
