using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dora.Interception
{
    internal sealed class InterceptableProxyFactoryCache : IInterceptableProxyFactoryCache
    {
        #region Fields  
        private readonly ICodeGeneratorFactory _codeGeneratorFactory;
        private readonly Dictionary<Type, Func<object, object>> _instanceFactories;
        private readonly Dictionary<Type, Func< IServiceProvider, object>> _typeFactories;
        #endregion

        #region Constructors
        public InterceptableProxyFactoryCache(ICodeGeneratorFactory  codeGeneratorFactory)
        {
            _codeGeneratorFactory = codeGeneratorFactory;
            _instanceFactories = new Dictionary<Type, Func<object,  object>>();
            _typeFactories = new Dictionary<Type, Func<IServiceProvider, object>>();
        }
        #endregion

        #region Public Methods           

        public Func<object, object> GetInstanceFactory(Type type, IInterceptorRegistry interceptors)
        {
            if (_instanceFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }
            lock (_instanceFactories)
            {
                if (_instanceFactories.TryGetValue(type, out factory))
                {
                    return factory;
                }
                var context = new CodeGenerationContext(type, interceptors);
                var proxyType = _codeGeneratorFactory.Create().GenerateInterceptableProxyClass(context);
                return _instanceFactories[type] = CreateInstanceFactory(proxyType, interceptors);
            }
        }

        public Func<IServiceProvider, object> GetTypeFactory(Type type, IInterceptorRegistry interceptors)
        {
            if (_typeFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }
            lock (_typeFactories)
            {
                if (_typeFactories.TryGetValue(type, out factory))
                {
                    return factory;
                }
                var context = new CodeGenerationContext(type, interceptors);
                var proxyType = _codeGeneratorFactory.Create().GenerateInterceptableProxyClass(context);
                return _typeFactories[type] = CreateTypeFactory(proxyType, interceptors);
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
        private Func<object, object> CreateInstanceFactory(Type proxyType, IInterceptorRegistry interceptors)
        {
            var target = Expression.Parameter(typeof(object));
            var constructor = proxyType.GetConstructors()[0];
            var targetType = constructor.GetParameters()[0].ParameterType;

            var convert = Expression.Convert(target, targetType);
            var create = Expression.New(constructor, convert, Expression.Constant(interceptors));
            return Expression
                .Lambda<Func<object, object>>(create, target)
                .Compile();
        }

        private Func<IServiceProvider, object> CreateTypeFactory(Type proxyType, IInterceptorRegistry interceptors)
        {
            return serviceProvider =>
            {
                var proxy = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, proxyType);
                ((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
        #endregion
    }
}
