using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public class InterceptorBuilder : IInterceptorBuilder
    {
        private readonly ConcurrentDictionary<Type, Func<object, InterceptorDelegate>> _delegateAccessors = new ConcurrentDictionary<Type, Func<object, InterceptorDelegate>>();
        private readonly ConcurrentDictionary<Type, Func<object, bool>> _captureArgumentsAccessors = new ConcurrentDictionary<Type, Func<object, bool>>();
        private readonly IServiceProviderAccessor _serviceProviderAccessor;

        private delegate Task ExecutorDelegate(object interceptor, InvocationContext context, IServiceProvider serviceProvider);
        private static readonly MethodInfo _getServiceMethod = typeof(InterceptorBuilder).GetMethod("GetService", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<Type, ExecutorDelegate> _executors = new Dictionary<Type, ExecutorDelegate>();
        private static readonly object _lock = new object();

        public InterceptorBuilder(IServiceProviderAccessor serviceProviderAccessor)
        {
            _serviceProviderAccessor = serviceProviderAccessor ?? throw new ArgumentNullException(nameof(serviceProviderAccessor));
        }

        public IInterceptor Build(IEnumerable<InterceptorRegistration> registrations)
        {
            var interceptors = registrations.Reverse().Select(it => CreateInterceptor(it)).ToArray();
            var captureArguments = interceptors.Any(it => it.CaptureArguments);
            InterceptorDelegate @delegate = next => {
                return context =>
                {
                    foreach (var interceptor in interceptors)
                    {
                        next = interceptor.Delegate(next);
                    }
                    return next(context);
                };
            };
            return new Interceptor(@delegate, captureArguments);
        }

        private Func<object, InterceptorDelegate> CreateDelegateAccessor(Type type)
        {
            return interceptor =>
            {
                var executor = GetExecutor(type);
                return next =>
                {
                    return context =>
                    {
                        context.Next = next;
                        return executor(interceptor, context, _serviceProviderAccessor.ServiceProvider);
                    };                    
                };
            };
        }

        private Func<object, bool> CreateCaptureArgumentsAccessors(Type type)
        {
            var property = type.GetProperty("CaptureArguments", BindingFlags.Public|  BindingFlags.Instance);
            var getMethod = property?.GetMethod;
            if (getMethod == null || getMethod.ReturnType != typeof(bool))
            {
                return _ => false;
            }

            var interceptor = Expression.Parameter(typeof(object));
            var cast = Expression.Convert(interceptor, type);
            var call = Expression.Call(cast, getMethod);
            return Expression.Lambda<Func<object, bool>>(Expression.Convert(call, typeof(bool)), interceptor).Compile();
        }

        private IInterceptor CreateInterceptor(InterceptorRegistration registration)
        {
            var interceptor = registration.InterceptorFactory(_serviceProviderAccessor.ServiceProvider);
            if (interceptor is IInterceptor interceptor1)
            {
                return interceptor1;
            }
            var type = interceptor.GetType();
            InterceptorClassVerifier.EnsureValidInterceptorClass(type);
            var delegateAccessor = _delegateAccessors.GetOrAdd(type, CreateDelegateAccessor);
            var captureArgumentsAccessor = _captureArgumentsAccessors.GetOrAdd(type, CreateCaptureArgumentsAccessors);
            return new Interceptor(delegateAccessor(interceptor), captureArgumentsAccessor(interceptor));
        }

        private static ExecutorDelegate GetExecutor(Type interceptorType)
        {
            if (_executors.TryGetValue(interceptorType, out var executor))
            {
                return executor;
            }

            lock (_lock)
            {
                if (_executors.TryGetValue(interceptorType, out executor))
                {
                    return executor;
                }

                var search = from it in interceptorType.GetTypeInfo().GetMethods()
                             let parameters = it.GetParameters()
                             where it.Name == "InvokeAsync" && it.ReturnType == typeof(Task) && parameters.Any(it=>it.ParameterType == typeof(InvocationContext))
                             select it;
                MethodInfo invokeAsyncMethod = search.SingleOrDefault();
                if (null == invokeAsyncMethod)
                {
                    throw new InvalidOperationException($"{interceptorType.FullName} is not a valid interceptor type.");
                }

                ParameterExpression interceptor = Expression.Parameter(typeof(object), "interceptor");
                ParameterExpression invocationContext = Expression.Parameter(typeof(InvocationContext), "invocationContext");
                ParameterExpression serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

                var arguments = invokeAsyncMethod.GetParameters().Select(it => GetArgument(invocationContext, serviceProvider, it.ParameterType));
                Expression instance = Expression.Convert(interceptor, interceptorType);
                var invoke = Expression.Call(instance, invokeAsyncMethod, arguments);
                executor = Expression.Lambda<ExecutorDelegate>(invoke, interceptor, invocationContext, serviceProvider).Compile();
                _executors[interceptorType] = executor;
            }
            return executor;
        }

        private static Expression GetArgument(Expression invocationContext, Expression serviceProvider, Type parameterType)
        {
            if (parameterType == typeof(InvocationContext))
            {
                return invocationContext;
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
