using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Method invocation context.
    /// </summary>
    public abstract class InvocationContext
    {
        /// <summary>
        /// Gets the target instance the method is finally called against.
        /// </summary>
        /// <value>
        /// The target instance the method is finally called against.
        /// </value>
        public object Target { get; } = default!;

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> representing the target method.
        /// </summary>
        /// <value>
        /// The <see cref="MethodInfo"/> representing the target method.
        /// </value>
        public abstract MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the method invocation scope based <see cref="IServiceProvider"/>.
        /// </summary>
        /// <value>
        /// The method invocation scope based <see cref="IServiceProvider"/>.
        /// </value>
        public abstract IServiceProvider InvocationServices { get; }

        /// <summary>
        /// Gets the dictionary used to attach any property data into the invocation context.
        /// </summary>
        /// <value>
        /// The dictionary used to attach any property data into the invocation context.
        /// </value>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Gets the argument value based on specified name.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <returns>The argument value.</returns>
        public abstract TArgument GetArgument<TArgument>(string name);

        /// <summary>
        /// Gets the argument value based on specified name.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="index">The parameter index (zero-based position).</param>
        /// <returns>The argument value.</returns>
        public abstract TArgument GetArgument<TArgument>(int index);

        /// <summary>
        /// Sets the argument value.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The argument value.</param>
        /// <returns>The current <see cref="InvocationContext"/>.</returns>
        public abstract InvocationContext SetArgument<TArgument>(string name, TArgument value);


        /// <summary>
        /// Sets the argument value.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <param name="index">The parameter index (zero-based position).</param>
        /// <param name="value">The argument value.</param>
        /// <returns>The current <see cref="InvocationContext"/>.</returns>
        public abstract InvocationContext SetArgument<TArgument>(int index, TArgument value);

        /// <summary>
        /// Gets the return value of method invocation.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <returns>The return value of method invocation.</returns>
        public abstract TReturnValue GetReturnValue<TReturnValue>();

        /// <summary>
        /// Sets the return value.
        /// </summary>
        /// <typeparam name="TReturnValue">The type of the return value.</typeparam>
        /// <param name="value">The return value of method invocation.</param>
        /// <returns>The current <see cref="InvocationContext"/>.</returns>
        public abstract InvocationContext SetReturnValue<TReturnValue>(TReturnValue value);

        /// <summary>
        /// Gets or sets the <see cref="InvokeDelegate"/> used to invoke the next interceptor or target method.
        /// </summary>
        /// <value>
        /// The <see cref="InvokeDelegate"/> used to invoke the next interceptor or target method.
        /// </value>
        internal InvokeDelegate Next { get; set; } = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        protected InvocationContext(object target) => Target = target ?? throw new ArgumentNullException(nameof(target));

        /// <summary>
        /// Call the next interceptor or target method.
        /// </summary>
        /// <returns>The <see cref="ValueTask"/> to call the next interceptor or target method.</returns>
        public ValueTask ProceedAsync() => Next.Invoke(this);
    }
}