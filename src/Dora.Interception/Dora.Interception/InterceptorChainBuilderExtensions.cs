using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public static class InterceptorChainBuilderExtensions
    {
        private delegate Task InvokeDelegate(object interceptor, InvocationContext context, IServiceProvider serviceProvider);

        private static MethodInfo _getServiceMethod = typeof(InterceptorChainBuilderExtensions).GetTypeInfo().GetMethod("GetService", BindingFlags.Static| BindingFlags.NonPublic);
        private static Dictionary<Type, InvokeDelegate> _invokers = new Dictionary<Type, InvokeDelegate>();
        private static object _syncHelper = new object();

        public static IInterceptorChainBuilder Use<TInterceptor>(this IInterceptorChainBuilder builder, int order, params object[] arguments)
        {
            return builder.Use(typeof(TInterceptor), order, arguments);
        }

        public static IInterceptorChainBuilder Use(this IInterceptorChainBuilder builder, Type interceptorType, int order, params object[] arguments)
        {
            InterceptorDelegate interceptor = next => (async context=>{
                object[] newArguments = new object[arguments.Length + 1];
                newArguments[0] = next;
                arguments.CopyTo(newArguments, 1);
                object instance = ActivatorUtilities.CreateInstance(builder.ServiceProvider, interceptorType, newArguments);
                InvokeDelegate invoker;
                if (TryGetInvoke(interceptorType, out invoker))
                {
                    await invoker(instance, context, builder.ServiceProvider);
                }
                else
                {
                    throw new ArgumentException("Invalid interceptor type", "interceptorType");
                }
            });
            return builder.Use(interceptor, order);
        }

        private static bool TryGetInvoke(Type interceptorType, out InvokeDelegate invoker)
        {
            if (_invokers.TryGetValue(interceptorType, out invoker))
            {
                return true;
            }

            lock (_syncHelper)
            {
                if (_invokers.TryGetValue(interceptorType, out invoker))
                {
                    return true;
                }

                var search = from it in interceptorType.GetTypeInfo().GetMethods()
                             let parameters = it.GetParameters()
                             where it.Name == "InvokeAsync" && it.ReturnType == typeof(Task) && parameters.FirstOrDefault()?.ParameterType == typeof(InvocationContext)
                             select it;
                MethodInfo invokeAsyncMethod = search.FirstOrDefault();
                if (null == invokeAsyncMethod)
                {
                    return false;
                }

                ParameterExpression interceptor = Expression.Parameter(typeof(object), "interceptor");
                ParameterExpression invocationContext = Expression.Parameter(typeof(InvocationContext), "invocationContext");
                ParameterExpression serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

                var arguments = invokeAsyncMethod.GetParameters().Select(it =>GetArgument(invocationContext, serviceProvider, it.ParameterType));
                Expression instance = Expression.Convert(interceptor, interceptorType);
                var invoke = Expression.Call(instance, invokeAsyncMethod, arguments);
                invoker = Expression.Lambda<InvokeDelegate>(invoke, interceptor, invocationContext, serviceProvider).Compile();
                _invokers[interceptorType] = invoker;
            }
            return true;
        }

        private static Expression GetArgument(Expression invocationContext, Expression serviceProvider, Type parameterType)
        {
            if (parameterType == typeof(InvocationContext))
            {
                return invocationContext;
            }
            Expression serviceType = Expression.Constant(parameterType, typeof(Type));
            Expression callGetService = Expression.Call(_getServiceMethod, serviceProvider,serviceType);
            return Expression.Convert(callGetService, parameterType);
        }
        private static object GetService(IServiceProvider serviceProvider,Type type)
        {
            return serviceProvider.GetService(type);
        }
    }
}
