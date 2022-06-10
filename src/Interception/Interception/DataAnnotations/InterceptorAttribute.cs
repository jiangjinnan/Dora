namespace Dora.Interception
{
    /// <summary>
    /// Attribute used to apply specified interceptor type to target methods.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class InterceptorAttribute : Attribute
    {
        /// <summary>
        /// Gets the interceptor type, the current type will be returned if not explicitly specified.
        /// </summary>
        /// <value>
        /// The interceptor type.
        /// </value>
        public Type Interceptor { get; }

        /// <summary>
        /// Gets the arguments passed to interceptor type's constructor when creating interceptor instance.
        /// </summary>
        /// <value>
        /// The arguments passed to interceptor type's constructor when creating interceptor instance.
        /// </value>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets or sets the order, which determines the target interceptor's position in the interceptor pipeline.
        /// </summary>
        /// <value>
        /// The order, which determines the target interceptor's position in the interceptor pipeline.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorAttribute"/> class.
        /// </summary>
        /// <param name="arguments">The arguments passed to interceptor type's constructor when creating interceptor instance.</param>
        public InterceptorAttribute(params object[] arguments) : this(null, arguments) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorAttribute"/> class.
        /// </summary>
        /// <param name="interceptor">The  interceptor type.</param>
        /// <param name="arguments">The arguments passed to interceptor type's constructor when creating interceptor instance.</param>
        public InterceptorAttribute(Type? interceptor, params object[] arguments)
        {
            Interceptor = interceptor ?? GetType();
            Arguments = arguments;
        }
    }
}
