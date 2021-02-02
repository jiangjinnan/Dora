using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public static class Members
    {
        public static ConstructorInfo ConstructorOfObject;
        public static ConstructorInfo ConstructorOfInvocationContext;
        public static ConstructorInfo ConstructorOfInvokerDelegate;
        public static ConstructorInfo ConstructorOfValueTask;

        public static MethodInfo CreateInstanceOfActivatorUtilities;
        public static MethodInfo GetMethodFromHandleOfMethodBase;
        public static MethodInfo GetInterceptorOfInterceptorProvider;
        public static MethodInfo GetCaptureArgumentsOfInterceptor;
        public static MethodInfo GetAlterArgumentsOfInterceptor;
        public static MethodInfo ExecuteInterceptorOfProxyGeneratorHelper;
        public static MethodInfo WaitOfTask;
        public static MethodInfo GetCompletedTaskOfTask;
        public static MethodInfo SetReturnValueOfInvocationContext;
        public static MethodInfo GetResultOfProxyGeneratorHelper;
        public static MethodInfo GetTaskOfResultOfProxyGeneratorHelper;
        public static MethodInfo GetValueTasOfProxyGeneratorHelper;
        public static MethodInfo GetValueTaskOfResultOfProxyGeneratorHelper;
        public static MethodInfo AsTaskOfValueTask;
        public static MethodInfo AsTaskOfValueTaskOfResult;
        public static MethodInfo PreserveOfValueTask;
        public static MethodInfo AsTaskByValueTaskOfProxyGeneratorHelper;
        public static MethodInfo AsTaskByValueTaskOfResultOfProxyGeneratorHelper;

        static Members()
        {
            CreateInstanceOfActivatorUtilities = ResolveGenericMethodDefinition(() => CreateInstance<string>(default));
            ConstructorOfObject = typeof(object).GetConstructor(Array.Empty<Type>());
            GetCompletedTaskOfTask = typeof(Task).GetProperty(nameof(Task.CompletedTask)).GetMethod;
            ConstructorOfValueTask = typeof(ValueTask).GetConstructor(new Type[] { typeof(Task) });

            GetMethodFromHandleOfMethodBase = ResolveMethodInfo(() => MethodBase.GetMethodFromHandle(default));
            GetInterceptorOfInterceptorProvider = ResolveMethodInfo<IInterceptorProvider>(p => p.GetInterceptor(default));
            GetCaptureArgumentsOfInterceptor = typeof(IInterceptor).GetProperty(nameof(IInterceptor.CaptureArguments)).GetMethod;
            GetAlterArgumentsOfInterceptor = typeof(IInterceptor).GetProperty(nameof(IInterceptor.AlterArguments)).GetMethod;
            ConstructorOfInvocationContext = typeof(InvocationContext).GetConstructor(new Type[] { typeof(object), typeof(MethodInfo), typeof(object[]) });
            ConstructorOfInvokerDelegate = typeof(InvokerDelegate).GetConstructors().Single();
            ExecuteInterceptorOfProxyGeneratorHelper = ResolveMethodInfo(() => ProxyGeneratorHelper.ExecuteInterceptor(default, default, default));
            WaitOfTask = ResolveMethodInfo<Task>(task => task.Wait());
            SetReturnValueOfInvocationContext = ResolveGenericMethodDefinition<InvocationContext>(context => context.SetReturnValue<int>(default));
            GetResultOfProxyGeneratorHelper = ResolveGenericMethodDefinition(()=>ProxyGeneratorHelper.GetResult<int>(default,default));
            GetTaskOfResultOfProxyGeneratorHelper = ResolveGenericMethodDefinition(() => ProxyGeneratorHelper.GetTaskOfResult<int>(default, default));
            GetValueTasOfProxyGeneratorHelper = ResolveMethodInfo(()=>ProxyGeneratorHelper.GetValueTask(default,default));
            GetValueTaskOfResultOfProxyGeneratorHelper = ResolveGenericMethodDefinition(() => ProxyGeneratorHelper.GetValueTaskOfResult<int>(default, default));
            AsTaskOfValueTask = ResolveMethodInfo<ValueTask>(valueTask => valueTask.AsTask());
            AsTaskOfValueTaskOfResult = ResolveGenericMethodDefinition(() => AsTask<int>());
            PreserveOfValueTask = ResolveMethodInfo<ValueTask>(valueTask => valueTask.Preserve());
            AsTaskByValueTaskOfProxyGeneratorHelper = ResolveMethodInfo(() => ProxyGeneratorHelper.AsTaskByValueTask(default, default));
            AsTaskByValueTaskOfResultOfProxyGeneratorHelper = ResolveGenericMethodDefinition(() => ProxyGeneratorHelper.AsTaskByValueTaskOfResult<int>(default, default));
        }

        public static MethodInfo AsTask<T>() => ResolveMethodInfo<ValueTask<T>>(valueTask => valueTask.AsTask());

        static MethodInfo ResolveMethodInfo(Expression<Action> expression)
        {
            return ((MethodCallExpression)(expression.Body)).Method;
        }
        static MethodInfo ResolveMethodInfo<T>(Expression<Action<T>> expression)
        {
            return ((MethodCallExpression)(expression.Body)).Method;
        }
        static MethodInfo ResolveGenericMethodDefinition(Expression<Action> expression)
        {
            return ((MethodCallExpression)(expression.Body)).Method.GetGenericMethodDefinition();
        }
        static MethodInfo ResolveGenericMethodDefinition<T>(Expression<Action<T>> expression)
        {
            return ((MethodCallExpression)(expression.Body)).Method.GetGenericMethodDefinition();
        }

        public static T CreateInstance<T>(IServiceProvider serviceProvider) => ActivatorUtilities.CreateInstance<T>(serviceProvider);
    }
}
