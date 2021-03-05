using Dora.Primitives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    public static class MethodInterceptorAssignerExtensions
    {
        public static IMethodInterceptorAssigner<TTarget> ToMethod<TTarget>(this IMethodInterceptorAssigner<TTarget> assigner, Expression<Action<TTarget>> methodCall, int order)
        {
            Guard.ArgumentNotNull(assigner, nameof(assigner));
            Guard.ArgumentNotNull(methodCall, nameof(methodCall));
            return assigner.To(((MethodCallExpression)methodCall.Body).Method, order);
        }

        public static IMethodInterceptorAssigner<TTarget> ToProperty<TTarget, TProperty>(this IMethodInterceptorAssigner<TTarget> assigner, Expression<Func<TTarget, TProperty>> propertyAccess, PropertyMethodKind propertyMethod, int order)
        {
            Guard.ArgumentNotNull(assigner, nameof(assigner));
            Guard.ArgumentNotNull(propertyAccess, nameof(propertyAccess));
            if (propertyAccess.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                switch (propertyMethod)
                {
                    case PropertyMethodKind.Get:
                        {
                            return assigner.To(propertyInfo.GetMethod, order);
                        }
                    case PropertyMethodKind.Set:
                        {
                            return assigner.To(propertyInfo.SetMethod, order);
                        }
                    default:
                        {
                            if (propertyInfo.GetMethod != null)
                            {
                                assigner.To(propertyInfo.GetMethod, order);
                            }
                            if (propertyInfo.SetMethod != null)
                            {
                                assigner.To(propertyInfo.SetMethod, order);
                            }
                            return assigner;
                        }
                }
            }
            throw new ArgumentException("It is not a valid property access expression.", nameof(propertyAccess));
        }
    }
}
