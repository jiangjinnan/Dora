using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception
{
    public static class IInterceptorAssignerExtensions
    {
        public static IInterceptorAssigner AssignTo<T>(this IInterceptorAssigner assigner, MethodInfo methodInfo, int order)
        {
            return assigner.AssignTo(typeof(T),methodInfo, order);
        }

        public static IInterceptorAssigner AssignToMethod<T>(this IInterceptorAssigner assigner, Expression<Action<T>> methodCall, int order)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            return assigner.AssignTo<T>(method, order);
        }

        public static IInterceptorAssigner AssignToProperty<T, TValue>(this IInterceptorAssigner assigner, Expression<Func<T, TValue>> propertyAccessor, PropertyMethodKind propertyMethod, int order)
        {
            if (!(propertyAccessor.Body is MemberExpression memberAccessor) || !(memberAccessor.Member is PropertyInfo propertyInfo))
            {
                throw new ArgumentException("Specified is not valid property accessing expression.", nameof(propertyAccessor));
            }
            switch (propertyMethod)
            {
                case PropertyMethodKind.Get:
                    {
                        return assigner.AssignTo<T>(propertyInfo.GetMethod, order);
                    }
                case PropertyMethodKind.Set:
                    {
                        return assigner.AssignTo<T>(propertyInfo.SetMethod, order);
                    }
                default:
                    {
                        assigner.AssignTo<T>(propertyInfo.GetMethod, order);
                        return assigner.AssignTo<T>(propertyInfo.SetMethod, order);
                    }
            }
        }
    }
}
