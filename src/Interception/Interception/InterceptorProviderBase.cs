using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get the interceptors applied specified method.
    /// </summary>
    public abstract class InterceptorProviderBase : IInterceptorProvider
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorProviderBase"/> class.
        /// </summary>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        /// <exception cref="System.ArgumentNullException">interceptorFactory</exception>
        protected InterceptorProviderBase(IConventionalInterceptorFactory interceptorFactory) => InterceptorFactory = interceptorFactory ?? throw new ArgumentNullException(nameof(interceptorFactory));

        /// <summary>
        /// Gets the factory to create interceptor based on specified type and optional arguments.
        /// </summary>
        /// <value>
        /// The factory to create interceptor based on specified type and optional arguments.
        /// </value>
        public IConventionalInterceptorFactory InterceptorFactory { get; }

        /// <summary>
        /// Determines specified method can be intercepted.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="method">The method to check.</param>
        /// <param name="suppressed">A <see cref="Boolean" />value indicating whether to suppress interception.</param>
        /// <returns>
        /// A <see cref="Boolean" />value indicating specified method is intercepted.
        /// </returns>
        public abstract bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed);

        /// <summary>
        /// Gets the interceptors applied specified method.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="method">The target method.</param>
        /// <returns>
        /// The <see cref="Sortable{InvokeDelegate}" /> represents the applied interceptors.
        /// </returns>
        public abstract IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method);

        /// <summary>
        /// Validates and ensure interceptors are applied to approriate members of specified type.
        /// </summary>
        /// <param name="targetType">The type whose methods may be intercepted.</param>
        /// <param name="methodValidator">A delegate used to ensure the method to which the interceptors are applied is interceptable.</param>
        /// <param name="propertyValidator">A delegate used to ensure the property to which the interceptors are applied is interceptable.</param>
        public virtual void Validate(Type targetType, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator) { }
    }
}
