using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Dora.DynamicProxy
{
    internal class DynamicProxyFactoryCache: IDynamicProxyFactoryCache
    {
        #region Fields  
        private ConcurrentDictionary<Type, Type> _generatedClasses;  
        private ConcurrentDictionary<Type, Func<object, InterceptorDecoration, object>> _instanceFactories;
        private ConcurrentDictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>> _typeFactories;    
        #endregion

        #region Constructors
        internal DynamicProxyFactoryCache()
        {
            _generatedClasses = new ConcurrentDictionary<Type, Type>();
            _instanceFactories = new ConcurrentDictionary<Type, Func<object, InterceptorDecoration, object>>();
            _typeFactories = new ConcurrentDictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>>();      
        }
        #endregion

        #region Public Methods           

        public Func<object, InterceptorDecoration, object> GetInstanceFactory(Type type, InterceptorDecoration interceptors)
        {
            return _instanceFactories.GetOrAdd(type, type1 =>
            {
                var proxyType = _generatedClasses.GetOrAdd(type1, type2 => DynamicProxyClassGenerator.CreateInterfaceGenerator(type, interceptors).GenerateProxyType());
                return this.CreateInstanceFactory(proxyType);
            });
        }

        public Func<InterceptorDecoration, IServiceProvider, object> GetTypeFactory(Type type, InterceptorDecoration interceptors)
        {
            return _typeFactories.GetOrAdd(type, type1 =>
            {
                var proxyType = _generatedClasses.GetOrAdd(type1, type2 => DynamicProxyClassGenerator.CreateVirtualMethodGenerator(type, interceptors).GenerateProxyType());
                return this.CreateTypeFactory(proxyType);
            });      
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
                var proxy = ActivatorUtilities.CreateInstance(serviceProvider, proxyType);
                ((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
        #endregion
    }
}
