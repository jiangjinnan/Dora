using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dora.DynamicProxy
{
    internal sealed class DynamicProxyFactoryCache: IDynamicProxyFactoryCache
    {
        #region Fields  
        private readonly Dictionary<Type, Func<object, InterceptorRegistry, object>> _instanceFactories;
        private readonly Dictionary<Type, Func<InterceptorRegistry, IServiceProvider, object>> _typeFactories;    
        #endregion

        #region Constructors
        internal DynamicProxyFactoryCache()
        {
            _instanceFactories = new Dictionary<Type, Func<object, InterceptorRegistry, object>>();
            _typeFactories = new Dictionary<Type, Func<InterceptorRegistry, IServiceProvider, object>>();      
        }
        #endregion

        #region Public Methods           

        public Func<object, InterceptorRegistry, object> GetInstanceFactory(Type type, InterceptorRegistry interceptors)
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
                var proxyType = DynamicProxyClassGenerator.CreateInterfaceGenerator(type, interceptors).GenerateProxyType();
                return _instanceFactories[type] = CreateInstanceFactory(proxyType);
            }
        }

        public Func<InterceptorRegistry, IServiceProvider, object> GetTypeFactory(Type type, InterceptorRegistry interceptors)
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
                var proxyType = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(type, interceptors).GenerateProxyType();
                return _typeFactories[type] = CreateTypeFactory(proxyType);
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
        private Func<object, InterceptorRegistry, object> CreateInstanceFactory(Type proxyType)
        {
            var target = Expression.Parameter(typeof(object));
            var interceptors = Expression.Parameter(typeof(InterceptorRegistry));
            
            var constructor = proxyType.GetConstructors()[0];
            var targetType = constructor.GetParameters()[0].ParameterType;

            var convert = Expression.Convert(target, targetType);
            var create = Expression.New(constructor, convert, interceptors);
            return Expression
                .Lambda<Func<object, InterceptorRegistry, object>>(create, target, interceptors)
                .Compile();
        }

        private Func<InterceptorRegistry, IServiceProvider, object> CreateTypeFactory(Type proxyType)
        {
            return (interceptors, serviceProvider) =>
            {
                var proxy = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, proxyType);
                ((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
        #endregion
    }
}
