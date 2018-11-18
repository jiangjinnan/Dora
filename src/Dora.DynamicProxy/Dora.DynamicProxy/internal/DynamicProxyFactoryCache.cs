using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dora.DynamicProxy
{
    internal class DynamicProxyFactoryCache: IDynamicProxyFactoryCache
    {
        #region Fields  
        private Dictionary<Type, Func<object, InterceptorDecoration, object>> _instanceFactories;
        private Dictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>> _typeFactories;    
        #endregion

        #region Constructors
        internal DynamicProxyFactoryCache()
        {
            _instanceFactories = new Dictionary<Type, Func<object, InterceptorDecoration, object>>();
            _typeFactories = new Dictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>>();      
        }
        #endregion

        #region Public Methods           

        public Func<object, InterceptorDecoration, object> GetInstanceFactory(Type type, InterceptorDecoration interceptors)
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

        public Func<InterceptorDecoration, IServiceProvider, object> GetTypeFactory(Type type, InterceptorDecoration interceptors)
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
        private Func<object, InterceptorDecoration, object> CreateInstanceFactory(Type proxyType)
        {
            var target = Expression.Parameter(typeof(object));
            var interceptors = Expression.Parameter(typeof(InterceptorDecoration));
            var typeToIntercept = Expression.Constant(proxyType);

            var constructor = proxyType.GetConstructors()[0];
            var targetType = constructor.GetParameters()[0].ParameterType;

            var convert = Expression.Convert(target, targetType);
            var create = Expression.New(constructor, convert, interceptors);
            return Expression
                .Lambda<Func<object, InterceptorDecoration, object>>(create, target, interceptors)
                .Compile();
        }

        private Func<InterceptorDecoration, IServiceProvider, object> CreateTypeFactory(Type proxyType)
        {
            return (interceptors, serviceProvider) =>
            {
                var proxy = serviceProvider.GetRequiredService(proxyType);
                //ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, proxyType);
                ((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
        #endregion
    }
}
