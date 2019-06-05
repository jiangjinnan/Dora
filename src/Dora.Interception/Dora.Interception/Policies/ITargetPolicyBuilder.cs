using System;
using System.Linq.Expressions;

namespace Dora.Interception.Policies
{
    /// <summary>
    /// Define methods to build a <see cref="TargetTypePolicy"/>.
    /// </summary>
    /// <typeparam name="T">The target type to which the <see cref="IInterceptorProvider"/> is applied to.</typeparam>
    public interface ITargetPolicyBuilder<T>
    {
        /// <summary>
        /// Builds a new <see cref="TargetTypePolicy"/>.
        /// </summary>
        /// <returns>The built <see cref="TargetTypePolicy"/>.</returns>
        TargetTypePolicy Build();

        /// <summary>
        /// Includes all members of the target type..
        /// </summary>
        /// <returns>The current <see cref="ITargetPolicyBuilder{T}"/></returns>
        ITargetPolicyBuilder<T> IncludeAllMembers();

        /// <summary>
        /// Includes specified method of the target type.
        /// </summary>
        /// <param name="methodInvocation">The included method based invocation expression.</param>  
        /// <returns>The current <see cref="ITargetPolicyBuilder{T}"/></returns>
        ITargetPolicyBuilder<T> IncludeMethod(Expression<Action<T>> methodInvocation);

        /// <summary>
        /// Exeludes specified method of the target type.
        /// </summary>
        /// <param name="methodInvocation">The excluded method based invocation expression.</param>  
        /// <returns>The current <see cref="ITargetPolicyBuilder{T}"/></returns>
        ITargetPolicyBuilder<T> ExcludeMethod(Expression<Action<T>> methodInvocation);

        /// <summary>
        /// Includes specified property of the target type.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="propertyAccessor">The included property value accessor based expression.</param>
        /// <param name="propertyMethod">A <see cref="PropertyMethod"/> indicating what is included is Get method, Set method or both.</param>      
        /// <returns>The current <see cref="ITargetPolicyBuilder{T}"/></returns>
        ITargetPolicyBuilder<T> IncludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod);

        /// <summary>
        /// Excludes specified property of the target type.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="propertyAccessor">The excluded property value accessor based expression.</param>
        /// <param name="propertyMethod">A <see cref="PropertyMethod"/> indicating what is excluded is Get method, Set method or both.</param>      
        /// <returns>The current <see cref="ITargetPolicyBuilder{T}"/></returns>
        ITargetPolicyBuilder<T> ExcludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod);
    }
}