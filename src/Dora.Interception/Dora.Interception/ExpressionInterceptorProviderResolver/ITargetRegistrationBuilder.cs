using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dora.Interception
{
    public interface ITargetRegistrationBuilder<T>
    {
        TargetRegistration Build();
        ITargetRegistrationBuilder<T> IncludeAllMembers();
        ITargetRegistrationBuilder<T> ExecluedAllMembers();
        ITargetRegistrationBuilder<T> IncludeMethod(Expression<Action<T>> methodInvocation);
        ITargetRegistrationBuilder<T> ExecludeMethod(Expression<Action<T>> methodInvocation);
        ITargetRegistrationBuilder<T> IncludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod);
        ITargetRegistrationBuilder<T> ExcludeProperty<TValue>(Expression<Func<T, TValue>> propertyAccessor, PropertyMethod propertyMethod);
    }
}
