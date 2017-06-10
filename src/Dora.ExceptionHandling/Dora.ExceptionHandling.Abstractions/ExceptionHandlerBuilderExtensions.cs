using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Dora.ExceptionHandling.Abstractions.Properties;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Define some extension methods specific to <see cref="IExceptionHandlerBuilder"/>.
    /// </summary>
    public static class ExceptionHandlerBuilderExtensions
    {
        private delegate Task HandleExceptionDelegate(object handler, ExceptionContext context, IServiceProvider serviceProvider);
        private static MethodInfo _getServiceMethod = typeof(ExceptionHandlerBuilderExtensions).GetTypeInfo().GetMethod("GetService", BindingFlags.Static | BindingFlags.NonPublic);
        private static Dictionary<Type, HandleExceptionDelegate> _handlers = new Dictionary<Type, HandleExceptionDelegate>();
        private static object _syncHelper = new object();

        /// <summary>
        /// Register specified type of exception handler to <see cref="IExceptionHandlerBuilder"/>.
        /// </summary>
        /// <typeparam name="THandler">The type of exception handler to register.</typeparam>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> to which the exception handler is registered.</param>
        /// <param name="arguments">The arguments passed to constrcutors.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="builder"/> is null.</exception>
        public static IExceptionHandlerBuilder Use<THandler>(this IExceptionHandlerBuilder builder, params object[] arguments)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.Use(typeof(THandler), arguments);
        }

        /// <summary>
        /// Register specified type of exception handler to <see cref="IExceptionHandlerBuilder"/>.
        /// </summary>
        /// <typeparam name="THandler">The type of exception handler to register.</typeparam>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> to which the exception handler is registered.</param>
        /// <param name="predicate">A predicate to indicate whether to invoke the registered handler.</param>
        /// <param name="arguments">The arguments passed to constrcutors.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="builder"/> is null.</exception>
        public static IExceptionHandlerBuilder Use<THandler>(this IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, params object[] arguments)
        {
            return builder.Use(predicate,typeof(THandler) , arguments);
        }

        /// <summary>
        /// Register specified type of exception handler to <see cref="IExceptionHandlerBuilder"/>.
        /// </summary>
        /// <param name="handlerType">The type of exception handler to register.</param>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> to which the exception handler is registered.</param>
        /// <param name="arguments">The arguments passed to constrcutors.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="handlerType"/> is null.</exception>
        public static IExceptionHandlerBuilder Use(this IExceptionHandlerBuilder builder, Type handlerType, params object[] arguments)
        {
            return builder.Use(_ => true, handlerType, arguments);
        }

        /// <summary>
        /// Register specified type of exception handler to <see cref="IExceptionHandlerBuilder"/>.
        /// </summary>
        /// <param name="handlerType">The type of exception handler to register.</param>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> to which the exception handler is registered.</param>
        /// <param name="arguments">The arguments passed to constrcutors.</param>
        /// <param name="predicate">A predicate to indicate whether to invoke the registered handler.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="handlerType"/> is null.</exception>
        public static IExceptionHandlerBuilder Use(this IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate,  Type handlerType, params object[] arguments)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            Guard.ArgumentNotNull(handlerType, nameof(handlerType));

            if (!TryGetInvoke(handlerType, out HandleExceptionDelegate handlerDelegate))
            {
                throw new ArgumentException(Resources.ExceptionInvalidExceptionHandlerType.Fill(handlerType.FullName, nameof(handlerType)));
            }

            Func<ExceptionContext, Task> handler = async context =>
            {
                if (predicate(context))
                {
                    object[] newArguments = new object[arguments.Length];
                    arguments.CopyTo(newArguments, 0);
                    object instance = ActivatorUtilities.CreateInstance(builder.ServiceProvider, handlerType, newArguments);
                    await handlerDelegate(instance, context, builder.ServiceProvider);
                }
            };
            return builder.Use(handler);
        }

        private static bool TryGetInvoke(Type handlerType, out HandleExceptionDelegate handler)
        {
            if (_handlers.TryGetValue(handlerType, out handler))
            {
                return true;
            }

            lock (_syncHelper)
            {
                if (_handlers.TryGetValue(handlerType, out handler))
                {
                    return true;
                }

                var search = from it in handlerType.GetTypeInfo().GetMethods()
                             let parameters = it.GetParameters()
                             where it.Name == "HandleExceptionAsync" && it.ReturnType == typeof(Task) && parameters.FirstOrDefault()?.ParameterType == typeof(ExceptionContext)
                             select it;
                MethodInfo handleExceptionAsyncMethod = search.FirstOrDefault();
                if (null == handleExceptionAsyncMethod)
                {
                    return false;
                }

                ParameterExpression handlerParameter = Expression.Parameter(typeof(object), "handler");
                ParameterExpression contextParameter = Expression.Parameter(typeof(ExceptionContext), "exceptionContext");
                ParameterExpression serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

                var arguments = handleExceptionAsyncMethod.GetParameters().Select(it => GetArgument(contextParameter, serviceProviderParameter, it.ParameterType));
                Expression instance = Expression.Convert(handlerParameter, handlerType);
                var handleExceptionAsync = Expression.Call(instance, handleExceptionAsyncMethod, arguments);
                handler = Expression.Lambda<HandleExceptionDelegate>(handleExceptionAsync, handlerParameter, contextParameter, serviceProviderParameter).Compile();
                _handlers[handlerType] = handler;
            }
            return true;
        }

        private static Expression GetArgument(Expression exceptionContext, Expression serviceProvider, Type parameterType)
        {
            if (parameterType == typeof(ExceptionContext))
            {
                return exceptionContext;
            }
            Expression serviceType = Expression.Constant(parameterType, typeof(Type));
            Expression callGetService = Expression.Call(_getServiceMethod, serviceProvider, serviceType);
            return Expression.Convert(callGetService, parameterType);
        }

        private static object GetService(IServiceProvider serviceProvider, Type type)
        {
            return serviceProvider.GetService(type);
        }
    }
}
