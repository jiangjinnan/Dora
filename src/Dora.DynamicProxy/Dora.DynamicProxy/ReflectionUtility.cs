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
        //private static Dictionary<Type, ConstructorInfo> _construcorOfReturnValueAccessors;
        //private static Dictionary<Type, ConstructorInfo> _construcorOfFuncOfTasksAndReturnValue;   

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
        
        public static MethodInfo InvokeHandlerMethod
        {
            get
            {
                return _invokeHandlerMethod ?? (_invokeHandlerMethod = GetMethod(() => TargetInvoker.InvokeHandler(null, null, null)));
            }
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
                return _constructorOfInterceptDelegate
                ?? (_constructorOfInterceptDelegate = typeof(InterceptDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            }
        } 

        //public static ConstructorInfo GetConstructorOfRetureValueAccessor<TResult>()
        //{
        //    return GetConstructor(() => new ReturnValueAccessor<TResult>(null)); 
        //}

        //public static ConstructorInfo GetConstructorOfRetureValueAccessor(Type returnType)
        //{
        //    _construcorOfReturnValueAccessors = _construcorOfReturnValueAccessors ?? new Dictionary<Type, ConstructorInfo>();
        //    if (_construcorOfReturnValueAccessors.TryGetValue(returnType, out var constructor))
        //    {
        //        return constructor;
        //    }

        //    return _construcorOfReturnValueAccessors[returnType] = typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetConstructor(new Type[] { typeof(InvocationContext) });
        //}

        //public static ConstructorInfo GetConstructorOfFuncOfTaskAndReturnValue(Type returnType)
        //{
        //    _construcorOfFuncOfTasksAndReturnValue = _construcorOfFuncOfTasksAndReturnValue ?? new Dictionary<Type, ConstructorInfo>();
        //    if (_construcorOfFuncOfTasksAndReturnValue.TryGetValue(returnType, out var constructor))
        //    {
        //        return constructor;
        //    } 
        //    return _construcorOfFuncOfTasksAndReturnValue[returnType] = typeof(Func<,>).MakeGenericType(typeof(Task),returnType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr)});
        //}
        public static MethodInfo GetMethodOfArgumentsPropertyOfInvocationContext
        {
            get
            {
                return _getMethodOfArgumentsOfInvocationContext
                     ?? (_getMethodOfArgumentsOfInvocationContext = GetProperty<InvocationContext, object[]>(_ => _.Arguments).GetMethod);
            }
        } 

        public static MethodInfo GetMethodOfCompletedTaskOfTask
        {
            get
            {
                return _getMethodOfCompletedTaskOfTask
                     ?? (_getMethodOfCompletedTaskOfTask = GetProperty<Task, Task>(_ => Task.CompletedTask).GetMethod);
            }
        } 
        
        public static MethodInfo GetMethodOfReturnValueOfInvocationContext
        {
            get
            {
                return _getMethodOfReturnValueOfInvocationContext
                     ?? (_getMethodOfReturnValueOfInvocationContext = GetProperty<InvocationContext, object>(_=>_.ReturnValue).GetMethod);
            }
        }

        public static MethodInfo SetMethodOfReturnValueOfInvocationContext
        {
            get
            {
                return _setMethodOfReturnValueOfInvocationContext
                     ?? (_setMethodOfReturnValueOfInvocationContext = GetProperty<InvocationContext, object>(_ => _.ReturnValue).SetMethod);
            }
        }
        public static MethodInfo GetMethodFromHandleMethodOfMethodBase1
        {
            get
            {
                return _getMethodFromHandleMethodOfMethodBase1
                ?? (_getMethodFromHandleMethodOfMethodBase1 = GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle))));
            }
        }

        public static MethodInfo GetMethodFromHandleMethodOfMethodBase2
        {
            get
            {
                return _getMethodFromHandleMethodOfMethodBase2
                ?? (_getMethodFromHandleMethodOfMethodBase2 = GetMethod<MethodBase>(_ => MethodBase.GetMethodFromHandle(default(RuntimeMethodHandle),default(RuntimeTypeHandle))));
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
        //public static MethodInfo GetMethodsOfGetReturnValue(Type returnType)
        //{
        //    _methodsOfGetReturnValues = _methodsOfGetReturnValues ?? new Dictionary<Type, MethodInfo>();
        //    if (_methodsOfGetReturnValues.TryGetValue(returnType, out var methodInfo))
        //    {
        //        return methodInfo;
        //    }

        //    return _methodsOfGetReturnValues[returnType] = typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetMethod("GetReturnValue", new Type[] { typeof(Task) });
        //}  

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

        
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TargetInvoker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interceptor"></param>
        /// <param name="handler"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task InvokeHandler(InterceptorDelegate interceptor, InterceptDelegate handler, InvocationContext context)
        {
            handler = interceptor(handler);
            return handler(context);
        }
    }
}
