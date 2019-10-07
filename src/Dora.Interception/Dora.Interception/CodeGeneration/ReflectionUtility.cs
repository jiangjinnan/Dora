using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    internal static class ReflectionUtility
    {
        #region Fields
        private static ConstructorInfo _constructorOfObject;
        private static ConstructorInfo _constructorOfDefaultInvocationContext;
        private static ConstructorInfo _constructorOfInterceptDelegate;

        private static MethodInfo _getMethodFromHandleMethodOfMethodBase1;
        private static MethodInfo _getMethodFromHandleMethodOfMethodBase2;
        private static MethodInfo _invokeMethodOfInterceptorDelegate;
        private static MethodInfo _invokeMethodOfInterceptDelegate;
        private static MethodInfo _waitMethodOfTask;
        private static MethodInfo _invokeHandlerMethod;
        private static MethodInfo _getMethodOfArgumentsOfInvocationContext;
        private static MethodInfo _getMethodOfCompletedTaskOfTask;
        private static MethodInfo _getMethodOfReturnValueOfInvocationContext;
        private static MethodInfo _setMethodOfReturnValueOfInvocationContext;
        #endregion

        #region Properties
        public static MethodInfo InvokeHandlerMethod
        {
            get { return _invokeHandlerMethod ?? (_invokeHandlerMethod = GetMethod(() => TargetInvoker.InvokeHandler(null, null, null))); }
        }
        public static ConstructorInfo ConstructorOfObject
        {
            get { return _constructorOfObject ?? (_constructorOfObject = GetConstructor(() => new object())); }
        }
        public static ConstructorInfo ConstructorOfDefaultInvocationContext
        {
            get
            {
                return _constructorOfDefaultInvocationContext
                ?? (_constructorOfDefaultInvocationContext = GetConstructor(() => new DefaultInvocationContext(null, null, null, null)));
            }
        }
        public static ConstructorInfo ConstructorOfInterceptDelegate
        {
            get
            {
                return _constructorOfInterceptDelegate ?? (_constructorOfInterceptDelegate = typeof(InterceptDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            }
        }
        public static MethodInfo GetMethodOfArgumentsPropertyOfInvocationContext
        {
            get { return _getMethodOfArgumentsOfInvocationContext ?? (_getMethodOfArgumentsOfInvocationContext = GetProperty<InvocationContext, object[]>(_ => _.Arguments).GetMethod); }
        }
        public static MethodInfo GetMethodOfCompletedTaskOfTask
        {
            get { return _getMethodOfCompletedTaskOfTask ?? (_getMethodOfCompletedTaskOfTask = GetProperty<Task, Task>(_ => Task.CompletedTask).GetMethod); }
        }
        public static MethodInfo GetMethodOfReturnValueOfInvocationContext
        {
            get { return _getMethodOfReturnValueOfInvocationContext ?? (_getMethodOfReturnValueOfInvocationContext = GetProperty<InvocationContext, object>(_ => _.ReturnValue).GetMethod); }
        }
        public static MethodInfo SetMethodOfReturnValueOfInvocationContext
        {
            get { return _setMethodOfReturnValueOfInvocationContext ?? (_setMethodOfReturnValueOfInvocationContext = GetProperty<InvocationContext, object>(_ => _.ReturnValue).SetMethod); }
        }
        public static MethodInfo GetMethodFromHandleMethodOfMethodBase1
        {
            get { return _getMethodFromHandleMethodOfMethodBase1 ?? (_getMethodFromHandleMethodOfMethodBase1 = GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle)))); }
        }
        public static MethodInfo GetMethodFromHandleMethodOfMethodBase2
        {
            get { return _getMethodFromHandleMethodOfMethodBase2 ?? (_getMethodFromHandleMethodOfMethodBase2 = GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle), default(RuntimeTypeHandle)))); }
        }
        public static MethodInfo InvokeMethodOfInterceptorDelegate
        {
            get { return _invokeMethodOfInterceptorDelegate ?? (_invokeMethodOfInterceptorDelegate = GetMethod<InterceptorDelegate>(_ => _.Invoke(null))); }
        }
        public static MethodInfo InvokeMethodOfInterceptDelegate
        {
            get { return _invokeMethodOfInterceptDelegate ?? (_invokeMethodOfInterceptDelegate = GetMethod<InterceptDelegate>(_ => _.Invoke(null))); }
        }
        public static MethodInfo WaitMethodOfTask
        {
            get { return _waitMethodOfTask ?? (_waitMethodOfTask = GetMethod<Task>(_ => _.Wait())); }
        }
        #endregion

        #region Public Methods
        public static ConstructorInfo GetConstructor<T>(Expression<Func<T>> newExpression)
        {
            return ((NewExpression)newExpression.Body).Constructor;
        }
        public static MethodInfo GetMethod<T>(Expression<Action<T>> methodCall)
        {
            return ((MethodCallExpression)methodCall.Body).Method;
        }
        public static MethodInfo GetMethod(Expression<Action> methodCall)
        {
            return ((MethodCallExpression)methodCall.Body).Method;
        }
        public static PropertyInfo GetProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> propertyAccessExpression)
        {
            return (PropertyInfo)((MemberExpression)propertyAccessExpression.Body).Member;
        }

        public static InterfaceMethodMapping GetInterfaceMapForGenericTypeDefinition(Type @interface, Type targetType)
        {
            var interfaceMethods = @interface.GetMethods(); 
            var targetMethods = new MethodInfo[interfaceMethods.Length];
            var implementedInterface = targetType.GetInterfaces().Single(it => it.IsGenericType && it.GetGenericTypeDefinition() == @interface.GetGenericTypeDefinition());
            var interfaceGenericParameters = ((TypeInfo)@interface).GenericTypeParameters;
            var targetGenericParameters = ((TypeInfo)targetType).GenericTypeParameters;
            var typeGenericParameterMap = new Dictionary<Type, Type>();
            for (int index = 0; index < interfaceGenericParameters.Length; index++)
            {
                typeGenericParameterMap[interfaceGenericParameters[index]] = targetGenericParameters[index];
            }

            var candidates = targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int index = 0; index < interfaceMethods.Length; index++)
            {
                var interfaceMethod = interfaceMethods[index];
                targetMethods[index] = candidates.SingleOrDefault(it => Match(implementedInterface, interfaceMethod, it, typeGenericParameterMap));
                if (targetMethods[index] == null)
                {
                    throw new InvalidOperationException("Cannot locate the implementation method.");
                }
            }

            return new InterfaceMethodMapping
            {
                InterfaceMethods = interfaceMethods,
                TargetMethods = targetMethods,
                InterfaceType = @interface,
                TargetType = targetType
            };
        }

        private static bool Match(
            Type @interface,
            MethodInfo interfaceMethod,
            MethodInfo targetMethod,
            Dictionary<Type, Type> typeGenericParameterMap)
        {
            
            if (!IsMethodNameMatched(@interface, interfaceMethod, targetMethod))
            {
                return false;
            }
            if (interfaceMethod.IsGenericMethod != targetMethod.IsGenericMethod)
            {
                return false;
            }

            var parameters1 = interfaceMethod.GetParameters();
            var parameters2 = targetMethod.GetParameters();
            if (parameters1.Length != parameters2.Length)
            {
                return false;
            }

            var map = new Dictionary<Type, Type>(typeGenericParameterMap);
            if (interfaceMethod.IsGenericMethod)
            {
                var genericParameters1 = interfaceMethod.GetGenericArguments();
                var genericParameters2 = targetMethod.GetGenericArguments();
                if (genericParameters1.Length != genericParameters2.Length)
                {
                    return false;
                }
                for (int index = 0; index < genericParameters1.Length; index++)
                {
                    map[genericParameters1[index]] = genericParameters2[index];
                }
            }

            for (int index = 0; index < parameters1.Length; index++)
            {
                if (!IsParameterTypeCompatible(parameters1[index].ParameterType, parameters2[index].ParameterType, map))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsMethodNameMatched(Type @interface, MethodInfo interfaceMethod, MethodInfo targetMethod)
        {
            if (interfaceMethod.Name == targetMethod.Name)
            {
                return true;
            }
            var interfaceName = interfaceMethod.DeclaringType.FullName;
            var interfacePrefix = interfaceName.Substring(0, interfaceName.IndexOf('`')).Replace("+", ".");
            var genericArguments = @interface.GenericTypeArguments.Select(it => it.Name);
            var targetMethodName = $"{interfacePrefix}<{string.Join(",", genericArguments)}>.{interfaceMethod.Name}";
            return targetMethod.Name == targetMethodName;
        }

        private static bool IsParameterTypeCompatible(Type parameterType1, Type parameterType2, Dictionary<Type, Type> genericParameterMap)
        {
            if (parameterType1 == parameterType2)
            {
                return true;
            }

            if (genericParameterMap.TryGetValue(parameterType1, out var type) && type == parameterType2)
            {
                return true;
            }

            if (!parameterType1.IsGenericType || !parameterType2.IsGenericType)
            {
                return false;
            }

            if (parameterType1.GetGenericTypeDefinition() != parameterType2.GetGenericTypeDefinition())
            {
                return false;
            }

            var genericArguments1 = parameterType1.GetGenericArguments();
            var genericArguments2 = parameterType2.GetGenericArguments();
            for (int index = 0; index < genericArguments1.Length; index++)
            {
                if (!IsParameterTypeCompatible(genericArguments1[index], genericArguments2[index], genericParameterMap))
                {
                    return false;
                }
            }
            return true;
        }
    }


    #endregion
}
