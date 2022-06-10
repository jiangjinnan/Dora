using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Utility class used to get specified type's member.
    /// </summary>
    public static class MemberUtilities
    {
        /// <summary>
        /// Gets the <see cref="MethodInfo"/> based on specified method call expression.
        /// </summary>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <returns>The <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetMethod(Expression<Action> methodCallExpression) => ((MethodCallExpression)methodCallExpression.Body).Method;

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> based on specified method call expression.
        /// </summary>
        /// <typeparam name="T">The type of target instance the method is called against.</typeparam>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <returns>The <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetMethod<T>(Expression<Action<T>> methodCallExpression)
        {
            if (methodCallExpression.Body is MethodCallExpression methodCall)
            { 
                return methodCall.Method;
            }
            throw new ArgumentException("What is specified is not a method call expression.", nameof(methodCallExpression));
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> based on specified property access expression.
        /// </summary>
        /// <typeparam name="TTarget">The type of target instance the method is called against.</typeparam>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns>The <see cref="MethodInfo"/>.</returns>
        public static PropertyInfo GetProperty<TTarget>(Expression<Func<TTarget, object>> propertyAccessExpression)
        {
            // Property
            if (propertyAccessExpression.Body is MemberExpression  memberExpression && memberExpression.Member is PropertyInfo property)
            {
                return property;
            }

            //Convert
            if (propertyAccessExpression.Body is UnaryExpression  unaryExpression && unaryExpression.Operand is MemberExpression memberExp && memberExp.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }

            throw new ArgumentException("What is specified is not a property access  expression.", nameof(propertyAccessExpression));
        }

        /// <summary>
        /// Determines whether [is interface or virtual method] [the specified method].
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if [is interface or virtual method] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInterfaceOrVirtualMethod(MethodInfo method)
        { 
            if(method.IsVirtual)
            {
                return true;
            }

            var type = method.DeclaringType!;
            foreach (var @interface in type.GetInterfaces())
            {
                var mapping = type.GetInterfaceMap(@interface);
                if (mapping.TargetMethods.Contains(method))
                {
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<MethodInfo, PropertyInfo> _propertyMap = new();

        /// <summary>
        /// Tries get the method specific property.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="propertyInfo">The<see cref="PropertyInfo"/> whost get/set method is the specified method.</param>
        /// <returns>A <see cref="Boolean"/> value indicating if property exists.</returns>
        public static bool TryGetProperty(MethodInfo method, out PropertyInfo? propertyInfo)
        {
            if (_propertyMap.TryGetValue(method, out var property))
            {
                propertyInfo = property;
                return true;
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                var propertyName = method.Name[4..];
                propertyInfo = method.DeclaringType!.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(it => it.Name == propertyName && (it.GetMethod == method || it.SetMethod == method));
                if (propertyInfo != null)
                {
                    _propertyMap[method] = propertyInfo;
                    return true;
                }
            }
            propertyInfo = null;
            return false;
        }
    }
}
