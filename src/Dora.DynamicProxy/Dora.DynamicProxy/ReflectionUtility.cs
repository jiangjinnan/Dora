using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    internal static class ReflectionUtility
    {
        private static ConstructorInfo _constructorOfObject;
        private static ConstructorInfo _constructorOfDefaultInvocationContext;
        private static ConstructorInfo _constructorOfInterceptDelegate;
        private static Dictionary<Type, ConstructorInfo> _construcorOfReturnValueAccessors;
        private static Dictionary<Type, ConstructorInfo> _construcorOfFuncOfTasksAndReturnValue;

        private static PropertyInfo _argumentsPropertyOfInvocationContext;
        private static PropertyInfo _completedTaskOfTask;
        private static PropertyInfo _returnValueOfInvocationContext;

        private static MethodInfo _getMethodFromHandleMethodOfMethodBase;
        private static MethodInfo _invokeMethodOfInterceptorDelegate;
        private static MethodInfo _invokeMethodOfInterceptDelegate;
        private static MethodInfo _definitionOfContiueWithMethodOfTask;
        private static MethodInfo _waitMethodOfTask;
        private static Dictionary<Type, MethodInfo> _methodsOfGetReturnValues;
        private static Dictionary<Type, MethodInfo> _methodOfContiueWithMethodOfTasks;

        public static ConstructorInfo ConstructorOfObject
        {
            get { return _constructorOfObject ?? (_constructorOfObject = GetConstructor(() => new object())); }
        }
        public static ConstructorInfo ConstructorOfDefaultInvocationContext
        {
            get
            {
                    return _constructorOfDefaultInvocationContext 
                    ?? (_constructorOfDefaultInvocationContext = GetConstructor(() => new  DefaultInvocationContext(null,null,null,null)));
            }
        }
        public static ConstructorInfo ConstructorOfInterceptDelegate
        {
            get
            {
                return _constructorOfInterceptDelegate
                ?? (_constructorOfInterceptDelegate = typeof(InterceptDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            }
        } 

        public static ConstructorInfo GetConstructorOfRetureValueAccessor<TResult>()
        {
            return GetConstructor(() => new ReturnValueAccessor<TResult>(null)); 
        }

        public static ConstructorInfo GetConstructorOfRetureValueAccessor(Type returnType)
        {
            _construcorOfReturnValueAccessors = _construcorOfReturnValueAccessors ?? new Dictionary<Type, ConstructorInfo>();
            if (_construcorOfReturnValueAccessors.TryGetValue(returnType, out var constructor))
            {
                return constructor;
            }

            return _construcorOfReturnValueAccessors[returnType] = typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetConstructor(new Type[] { typeof(InvocationContext) });
        }

        public static ConstructorInfo GetConstructorOfFuncOfTaskAndReturnValue(Type returnType)
        {
            _construcorOfFuncOfTasksAndReturnValue = _construcorOfFuncOfTasksAndReturnValue ?? new Dictionary<Type, ConstructorInfo>();
            if (_construcorOfFuncOfTasksAndReturnValue.TryGetValue(returnType, out var constructor))
            {
                return constructor;
            }

            return _construcorOfFuncOfTasksAndReturnValue[returnType] = typeof(Func<,>).MakeGenericType(typeof(Task),returnType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr)});
        }
        public static PropertyInfo ArgumentsPropertyOfInvocationContext
        {
            get
            {
                return _argumentsPropertyOfInvocationContext
                     ?? (_argumentsPropertyOfInvocationContext = GetProperty<InvocationContext, object[]>(_ => _.Arguments));
            }
        }
        public static PropertyInfo CompletedTaskOfTask
        {
            get
            {
                return _completedTaskOfTask
                     ?? (_completedTaskOfTask = GetProperty<Task, Task>(_ => Task.CompletedTask));
            }
        }     
        public static PropertyInfo ReturnValueOfInvocationContext
        {
            get
            {
                return _returnValueOfInvocationContext
                     ?? (_returnValueOfInvocationContext = GetProperty<InvocationContext, object>(_=>_.ReturnValue));
            }
        } 
        public static MethodInfo GetMethodFromHandleMethodOfMethodBase
        {
            get
            {
                    return _getMethodFromHandleMethodOfMethodBase 
                    ?? (_getMethodFromHandleMethodOfMethodBase = GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle))));
            }
        } 
        public static MethodInfo InvokeMethodOfInterceptorDelegate
        {
            get
            {
                return _invokeMethodOfInterceptorDelegate
                ?? (_invokeMethodOfInterceptorDelegate = GetMethod<InterceptorDelegate>(_ => _.Invoke(null)));
            }
        }    
        public static MethodInfo InvokeMethodOfInterceptDelegate
        {
            get
            {
                return _invokeMethodOfInterceptDelegate
                ?? (_invokeMethodOfInterceptDelegate = GetMethod<InterceptDelegate>(_ => _.Invoke(null)));
            }
        }
        public static MethodInfo WaitMethodOfTask
        {
            get
            {
                return _waitMethodOfTask
                    ?? (_waitMethodOfTask = GetMethod<Task>(_ => _.Wait()));
            }
        }
        public static MethodInfo GetMethodOfContiueWithMethodOfTask(Type returnType)
        {
            _methodOfContiueWithMethodOfTasks = _methodOfContiueWithMethodOfTasks ?? new Dictionary<Type, MethodInfo>();
            if (_methodOfContiueWithMethodOfTasks.TryGetValue(returnType, out var methodInfo))
            {
                return methodInfo;
            }

            _definitionOfContiueWithMethodOfTask = _definitionOfContiueWithMethodOfTask
                ?? GetMethod<Task>(_ => _.ContinueWith(task => 1)).GetGenericMethodDefinition();

            return _methodOfContiueWithMethodOfTasks[returnType] = _definitionOfContiueWithMethodOfTask.MakeGenericMethod(returnType);

        }
        public static MethodInfo GetMethodsOfGetReturnValue(Type returnType)
        {
            _methodsOfGetReturnValues = _methodsOfGetReturnValues ?? new Dictionary<Type, MethodInfo>();
            if (_methodsOfGetReturnValues.TryGetValue(returnType, out var methodInfo))
            {
                return methodInfo;
            }

            return _methodsOfGetReturnValues[returnType] = typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetMethod("GetReturnValue", new Type[] { typeof(Task) });
        }

        public static ConstructorInfo GetConstructor<T>(Expression<Func<T>> newExpression)
        {   
            return ((NewExpression)newExpression.Body).Constructor;
        }     
        public static MethodInfo GetMethod<T>(Expression<Action<T>> methodCall)
        {
            return ((MethodCallExpression)methodCall.Body).Method;
        }  

        public static PropertyInfo GetProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> propertyAccessExpression)
        {
            return (PropertyInfo)((MemberExpression)propertyAccessExpression.Body).Member;
        }
    }
}
