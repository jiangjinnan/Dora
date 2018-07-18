using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    internal class TargetRegistrationBuilder<T> : ITargetRegistrationBuilder<T>
    {
        private readonly TargetRegistration _registration;    
        public TargetRegistrationBuilder()   => _registration = new TargetRegistration(typeof(T));
        public TargetRegistration Build() => _registration;

        public ITargetRegistrationBuilder<T> ExcludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod)
        {
            var property = (PropertyInfo)((MemberExpression)propertyAccessor.Body).Member;
            _registration.ExludedProperties[property.MetadataToken] = propertyMethod;
            return this;
        }

        public ITargetRegistrationBuilder<T> ExecludeMethod(Expression<Action<T>> methodInvocation)
        {
            var method = ((MethodCallExpression)(methodInvocation.Body)).Method;
            _registration.ExludedMethods.Add(method.MetadataToken);
            return this;
        }

        public ITargetRegistrationBuilder<T> ExecluedAllMembers()
        {
            _registration.ExludedAllMembers = true;
            return this;
        }

        public ITargetRegistrationBuilder<T> IncludeAllMembers()
        {
            _registration.IncludedAllMembers = true;
            return this;
        }

        public ITargetRegistrationBuilder<T> IncludeMethod(Expression<Action<T>> methodInvocation)
        {
            var method = ((MethodCallExpression)(methodInvocation.Body)).Method;
            _registration.IncludedMethods.Add(method.MetadataToken);
            return this;
        }

        public ITargetRegistrationBuilder<T> IncludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod)
        {
            var property = (PropertyInfo)((MemberExpression)propertyAccessor.Body).Member;
            _registration.IncludedProperties[property.MetadataToken] = propertyMethod;
            return this;
        }
    }
}
