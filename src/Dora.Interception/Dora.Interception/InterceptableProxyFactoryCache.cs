using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Dora.Interception
{
    internal sealed class InterceptableProxyFactoryCache : IInterceptableProxyFactoryCache
    {
        #region Fields  
        private readonly ReaderWriterLockSlim _instanceLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _typeLock = new ReaderWriterLockSlim();
        private readonly ICodeGeneratorFactory _codeGeneratorFactory;
        private readonly Dictionary<Tuple<Type, Type>, Func<object, object>> _instanceFactories;
        private readonly Dictionary<Type, Func<IServiceProvider, object>> _typeFactories;
        private readonly IInterceptorResolver _interceptorResolver;
        #endregion

        #region Constructors
        public InterceptableProxyFactoryCache(
            ICodeGeneratorFactory codeGeneratorFactory,
            IInterceptorResolver interceptorResolver)
        {
            _codeGeneratorFactory = codeGeneratorFactory;
            _interceptorResolver = interceptorResolver;
            _instanceFactories = new Dictionary<Tuple<Type, Type>, Func<object, object>>();
            _typeFactories = new Dictionary<Type, Func<IServiceProvider, object>>();
        }
        #endregion

        #region Public Methods           

        public Func<object, object> GetInstanceFactory(Type @interface, Type targetType)
        {
            var key = new Tuple<Type, Type>(@interface, targetType);
            _instanceLock.EnterReadLock();
            try
            {
                if (_instanceFactories.TryGetValue(key, out var factory))
                {
                    return factory;
                }
            }
            finally
            {
                _instanceLock.ExitReadLock();
            }
            _instanceLock.EnterWriteLock();
            try
            {
                if (_instanceFactories.TryGetValue(key, out var factory))
                {
                    return factory;
                }
                var context = new CodeGenerationContext(@interface, targetType, _interceptorResolver.GetInterceptors(@interface, targetType));
                var proxyType = _codeGeneratorFactory.Create().GenerateInterceptableProxyClass(context);
                return _instanceFactories[key] = CreateInstanceFactory(proxyType);
            }
            finally
            {
                _instanceLock.ExitWriteLock();
            }
        }

        public Func<IServiceProvider, object> GetTypeFactory(Type type)
        {
            _typeLock.EnterReadLock();
            try
            {
                if (_typeFactories.TryGetValue(type, out var factory))
                {
                    return factory;
                }
            }
            finally
            {
                _typeLock.ExitReadLock();
            }
            _typeLock.EnterWriteLock();
            try
            {
                if (_typeFactories.TryGetValue(type, out var factory))
                {
                    return factory;
                }
                var interceptors = _interceptorResolver.GetInterceptors(type);
                var context = new CodeGenerationContext(type, interceptors);
                var proxyType = _codeGeneratorFactory.Create().GenerateInterceptableProxyClass(context);
                return _typeFactories[type] = CreateTypeFactory(proxyType);
            }
            finally
            {
                _typeLock.ExitWriteLock();
            }
        }
        #endregion

        #region Private Methods        
        /// <summary>
        /// Creates the instance factory.
        /// </summary>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <returns></returns>\
        /// <example>
        /// public class FoobarProxy
        /// {
        ///     private Foobar _target;
        ///     private InterceptorRegistry _interceptors;
        ///     public (FoobarProxy target, InterceptorRegistry interceptors)
        ///     {
        ///         _target = target;
        ///         _interceptors = interceptors;
        ///     }
        /// }
        /// </example>
        private Func<object, object> CreateInstanceFactory(Type proxyType)
        {
            var target = Expression.Parameter(typeof(object));
            var constructor = proxyType.GetConstructors()[0];
            var targetType = constructor.GetParameters()[0].ParameterType;

            var convert = Expression.Convert(target, targetType);
            var create = Expression.New(constructor, convert, Expression.Constant(_interceptorResolver));
            return Expression
                .Lambda<Func<object, object>>(create, target)
                .Compile();
        }

        private Func<IServiceProvider, object> CreateTypeFactory(Type proxyType)
        {
            return serviceProvider =>
            {
                var proxy = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, proxyType);
                //((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
        #endregion
    }
}