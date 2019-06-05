using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// The default implementation of <see cref="InvocationContext"/>
    /// </summary>   
    public sealed class DefaultInvocationContext : InvocationContext
    {
        #region Properties
        /// <summary>
        /// Gets the <see cref="MethodInfo" /> representing the method being invoked on the proxy.
        /// </summary>
        public override MethodInfo Method { get; }

        /// <summary>
        /// Gets the proxy object on which the intercepted method is invoked.
        /// </summary>
        public override object Proxy { get; }

        /// <summary>
        /// Gets the object on which the invocation is performed.
        /// </summary>
        /// <remarks>For virtual method based interception, the <see cref="Proxy"/> and <see cref="Target"/> are the same.</remarks>
        public override object Target { get; }

        /// <summary>
        /// Gets the arguments that target method has been invoked with.
        /// </summary>
        /// <remarks>
        /// Each argument is writable.
        /// </remarks>
        public override object[] Arguments { get; }

        /// <summary>
        /// Gets or sets the return value of the method.
        /// </summary>
        public override object ReturnValue { get; set; }

        /// <summary>
        /// Gets the extended properties.
        /// </summary>
        /// <value>
        /// The extended properties.
        /// </value>
        public override IDictionary<string, object> ExtendedProperties { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInvocationContext"/> class.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo" /> representing the method being invoked on the proxy.</param>
        /// <param name="proxy">The proxy object on which the intercepted method is invoked.</param>
        /// <param name="target">The object on which the invocation is performed.</param>
        /// <param name="arguments">The arguments that target method has been invoked with.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="method"/> is null.</exception>   
        /// <exception cref="ArgumentNullException">The specified<paramref name="proxy"/> is null.</exception>   
        /// <exception cref="ArgumentNullException">The specified<paramref name="target"/> is null.</exception>   
        /// <exception cref="ArgumentNullException">The specified<paramref name="arguments"/> is null.</exception>
        public DefaultInvocationContext(
             MethodInfo method,  
             object proxy,
             object target,
             object[] arguments)
        {
            Method = Guard.ArgumentNotNull(method, nameof(method));
            Proxy = Guard.ArgumentNotNull(proxy, nameof(proxy));
            Target = Guard.ArgumentNotNull(target, nameof(target));
            Arguments = Guard.ArgumentNotNull(arguments, nameof(arguments));
            ExtendedProperties = new Dictionary<string, object>();  
        }
        #endregion
    }
}
