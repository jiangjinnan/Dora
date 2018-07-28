using Dora.Interception.Properties;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception.Policies
{
    internal class TargetPolicyBuilder<T> : ITargetPolicyBuilder<T>
    {
        private readonly TargetTypePolicy _policy = new TargetTypePolicy(typeof(T));    
        public TargetTypePolicy Build() => _policy;
        public ITargetPolicyBuilder<T> IncludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod)
        {
            PropertyInfo property = GetProperty(propertyAccessor);
            _policy.IncludedProperties[property.MetadataToken] = propertyMethod;
            return this;
        }
        public ITargetPolicyBuilder<T> ExcludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod)
        {
            Guard.ArgumentNotNull(propertyAccessor, nameof(propertyAccessor));
            var expression = propertyAccessor.Body as MemberExpression;
            if (null == expression)
            {
                throw new ArgumentException(Resources.NotPropertyAccessExpression, nameof(propertyAccessor));
            }
            var property = expression.Member as PropertyInfo;
            if (null == property)
            {
                throw new ArgumentException(Resources.NotPropertyAccessExpression, nameof(propertyAccessor));
            }
            _policy.ExludedProperties[property.MetadataToken] = propertyMethod;
            return this;
        }  
        public ITargetPolicyBuilder<T> IncludeMethod(Expression<Action<T>> methodInvocation)
        {
            MethodCallExpression expression = GetMetehod(methodInvocation);
            _policy.IncludedMethods.Add(expression.Method.MetadataToken);
            return this;
        }       
        public ITargetPolicyBuilder<T> ExecludeMethod(Expression<Action<T>> methodInvocation)
        {
            Guard.ArgumentNotNull(methodInvocation, nameof(methodInvocation));
            var expression = methodInvocation.Body as MethodCallExpression;
            if (null == expression)
            {
                throw new ArgumentException(Resources.NotMethodCallExpression, nameof(methodInvocation));
            }
            _policy.ExludedMethods.Add(expression.Method.MetadataToken);
            return this;
        }    
        public ITargetPolicyBuilder<T> IncludeAllMembers()
        {
            _policy.IncludedAllMembers = true;
            return this;
        }
        private static MethodCallExpression GetMetehod(Expression<Action<T>> methodInvocation)
        {
            Guard.ArgumentNotNull(methodInvocation, nameof(methodInvocation));
            var expression = methodInvocation.Body as MethodCallExpression;
            if (null == expression)
            {
                throw new ArgumentException(Resources.NotMethodCallExpression, nameof(methodInvocation));
            }

            return expression;
        }   
        private static PropertyInfo GetProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor)
        {
            Guard.ArgumentNotNull(propertyAccessor, nameof(propertyAccessor));
            var expression = propertyAccessor.Body as MemberExpression;
            if (null == expression)
            {
                throw new ArgumentException(Resources.NotPropertyAccessExpression, nameof(propertyAccessor));
            }
            var property = expression.Member as PropertyInfo;
            if (null == property)
            {
                throw new ArgumentException(Resources.NotPropertyAccessExpression, nameof(propertyAccessor));
            }

            return property;
        }
    }
}
