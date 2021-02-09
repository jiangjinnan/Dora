using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Method invocation context.
    /// </summary>
    public sealed class InvocationContext
    {
        /// <summary>
        /// Gets the object current method is invoked against.
        /// </summary>
        /// <value>
        /// The object current method is invoked against.
        /// </value>
        public object Target { get; }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets the return value.
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public object ReturnValue { get; private set; }

        internal InvokerDelegate Next { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationContext"/> class.
        /// </summary>
        /// <param name="target">The object current method is invoked against.</param>
        /// <param name="method">The current method.</param>
        /// <param name="arguments">The arguments.</param>
        public InvocationContext(object target, MethodInfo method, object[] arguments = null)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Method = method?? throw new ArgumentNullException(nameof(method));
            Arguments = arguments;
        }

        /// <summary>
        /// Invokes the subsequential interceptor chain, including target method.
        /// </summary>
        /// <returns>The task to invoke the subsequential interceptor chain, including target method.</returns>
        public Task InvokeAsync() => Next?.Invoke(this);

        /// <summary>
        /// Sets the return value.
        /// </summary>
        /// <typeparam name="T">The type of return value to set.</typeparam>
        /// <param name="value">The return value to set.</param>
        public void SetReturnValue<T>(T value) => ReturnValue = value;

        /// <summary>
        /// Gets the return value.
        /// </summary>
        /// <typeparam name="T">The type of return value to set.</typeparam>
        /// <returns>The return value.</returns>
        public T GetReturnValue<T>() => (T)ReturnValue;
    }
}
