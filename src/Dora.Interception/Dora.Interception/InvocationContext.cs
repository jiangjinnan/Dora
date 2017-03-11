using System;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Represents the invocation context specific to calling the proxy.
    /// </summary>
    public abstract class InvocationContext
    {
        /// <summary>
        /// Gets the arguments that target method has been invoked with.
        /// </summary>
        public abstract object[] Arguments { get; }

        /// <summary>
        /// Gets the generic arguments of the method.
        /// </summary>
        public abstract Type[] GenericArguments { get; }

        /// <summary>
        /// Gets the object on which the invocation is performed.
        /// </summary>
        public abstract object InvocationTarget { get; }

        /// <summary>
        ///  Gets the <see cref="MethodInfo"/> representing the method being invoked on the proxy.
        /// </summary>
        public abstract MethodInfo Method { get; }

        /// <summary>
        /// For interface proxies, this will point to the <see cref="MethodInfo"/> on the target class.
        /// </summary>
        public abstract MethodInfo MethodInvocationTarget { get; }

        /// <summary>
        /// Gets the proxy object on which the intercepted method is invoked.
        /// </summary>
        public abstract object Proxy { get; }

        /// <summary>
        /// Gets or sets the return value of the method.
        /// </summary>
        public abstract object ReturnValue { get; set; }

        /// <summary>
        ///  Gets the type of the target object for the intercepted method.
        /// </summary>
        public abstract Type TargetType { get; }

        /// <summary>
        ///  Gets the value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value of the argument at the specified index.</returns>
        public abstract object GetArgumentValue(int index);

        /// <summary>
        ///  Overrides the value of an argument at the given index with the new value provided.
        /// </summary>
        /// <param name="index">The index of the argument to override.</param>
        /// <param name="value">The new value for the argument.</param>
        /// <remarks> This method accepts an <see cref="object"/>, however the value provided must be compatible with the type of the argument defined on the method, otherwise an exception will be thrown.</remarks>
        public abstract void SetArgumentValue(int index, object value);
    }
}
