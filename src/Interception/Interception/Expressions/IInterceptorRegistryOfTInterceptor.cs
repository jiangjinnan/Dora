using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception.Expressions
{
    /// <summary>
    /// A registry used to register interceptors.
    /// </summary>
    /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
    public interface IInterceptorRegistry<TInterceptor>
    {
        /// <summary>
        /// Applies specified interceptor to all methods of target method.
        /// </summary>
        /// <typeparam name="TTarget">The type of target method to which the interceptor is applied.</typeparam>
        /// <param name="order">The order determining the specified interceptor in chain.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToAllMethods<TTarget>(int order);

        /// <summary>
        /// Applies specified interceptor to target method.
        /// </summary>
        /// <typeparam name="TTarget">The type of target method to which the interceptor is applied.</typeparam>
        /// <param name="order">The order determining the specified interceptor in chain.</param>
        /// <param name="methodCall">The expression to call the target method.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToMethod<TTarget>(int order, Expression<Action<TTarget>> methodCall);

        /// <summary>
        /// Applies specified interceptor to target method.
        /// </summary>
        /// <param name="order">The order determining the specified interceptor in chain.</param>
        /// <param name="targetType">The type of target property to which the interceptor is applied.</param>
        /// <param name="method">The target method to which specified interceptor is applied.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToMethod(int order, Type targetType, MethodInfo method);

        /// <summary>
        /// Applies specified interceptor to target property's get-method.
        /// </summary>
        /// <typeparam name="TTarget">The type of target property to which the interceptor is applied.</typeparam>
        /// <param name="order">The order.</param>
        /// <param name="propertyAccessor">The expression to access the target.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToGetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);

        /// <summary>
        /// Applies specified interceptor to target property's set-method.
        /// </summary>
        /// <typeparam name="TTarget">The type of target property to which the interceptor is applied.</typeparam>
        /// <param name="order">The order.</param>
        /// <param name="propertyAccessor">The expression to access the target.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToSetMethod<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);

        /// <summary>
        /// Applies specified interceptor to target property's set-method and get-method.
        /// </summary>
        /// <typeparam name="TTarget">The type of target property to which the interceptor is applied.</typeparam>
        /// <param name="order">The order.</param>
        /// <param name="propertyAccessor">The expression to access the target.</param>
        /// <returns>The current <see cref="IInterceptorRegistry{TInterceptor}"/></returns>
        IInterceptorRegistry<TInterceptor> ToProperty<TTarget>(int order, Expression<Func<TTarget, object?>> propertyAccessor);
    }
}
