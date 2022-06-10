using Dora.Interception.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Expression based intercepter registry.
    /// </summary>
    public interface IInterceptorRegistry
    {
        /// <summary>
        /// Create a <see cref="IInterceptorRegistry{TInterceptor}"/> to register specified interceptor type.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor to register.</typeparam>
        /// <param name="arguments">The arguments passed to interceptor constructor.</param>
        /// <returns>The <see cref="IInterceptorRegistry{TInterceptor}"/>.</returns>
        IInterceptorRegistry<TInterceptor> For<TInterceptor>(params object[] arguments);

        /// <summary>
        /// Supresses the type to be intercepted.
        /// </summary>
        /// <typeparam name="TTarget">The type to be suppressed.</typeparam>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressType<TTarget>();

        /// <summary>
        /// Supresses the types to be intercepted.
        /// </summary>
        /// <param name="types">The suprressed methods.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressTypes(params Type[] types);

        /// <summary>Supresses the method to be intercepted.</summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="methodCall">The method call expression against to the suprressed method.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressMethod<TTarget>(Expression<Action<TTarget>> methodCall);

        /// <summary>Supresses the methods to be intercepted.</summary>
        /// <param name="methods">The suprressed methods.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressMethods(params MethodInfo[] methods);

        /// <summary>
        /// Supresses the property to be suppressed.
        /// </summary>
        /// <typeparam name="TTarget">The type whose property is suppressed.</typeparam>
        /// <param name="propertyAccessor">The property access expression against the suppressed property.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressProperty<TTarget>(Expression<Func<TTarget, object>> propertyAccessor);

        /// <summary>
        /// Supresses the set method of property to be suppressed.
        /// </summary>
        /// <typeparam name="TTarget">The type whose property is suppressed.</typeparam>
        /// <param name="propertyAccessor">The property access expression against the suppressed property.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressSetMethod<TTarget>(Expression<Func<TTarget, object>> propertyAccessor);

        /// <summary>
        /// Supresses the get method of property to be suppressed.
        /// </summary>
        /// <typeparam name="TTarget">The type whose property is suppressed.</typeparam>
        /// <param name="propertyAccessor">The property access expression against the suppressed property.</param>
        /// <returns>The current <see cref="IInterceptorRegistry"/>.</returns>
        IInterceptorRegistry SupressGetMethod<TTarget>(Expression<Func<TTarget, object>> propertyAccessor);
    }
}
