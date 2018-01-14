using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dora.DynamicProxy
{
    internal class DynamicProxyFactoryCache
    {
        private static readonly DynamicProxyFactoryCache _instance = new DynamicProxyFactoryCache();
        private Dictionary<Type, Type> _generatedClasses;  
        private Dictionary<Type, Func<object, InterceptorDecoration, object>> _instanceFactories;
        private Dictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>> _typeFactories;
        private object _sync;

        private DynamicProxyFactoryCache()
        {
            _generatedClasses = new Dictionary<Type, Type>();
            _instanceFactories = new Dictionary<Type, Func<object, InterceptorDecoration, object>>();
            _typeFactories = new Dictionary<Type, Func<InterceptorDecoration, IServiceProvider, object>>();
            _sync = new object();
        }

        public static DynamicProxyFactoryCache Instance { get => _instance; }

        public Func<object, InterceptorDecoration, object> GetInstanceFactory(Type type, InterceptorDecoration interceptors)
        {
            if (_instanceFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }

            lock (_sync)
            {
                if (_instanceFactories.TryGetValue(type, out factory))
                {
                    return factory;
                }

                if (!_generatedClasses.TryGetValue(type, out var proxyType))
                {
                    proxyType = _generatedClasses[type] = DynamicProxyClassGenerator.CreateInterfaceGenerator(type, interceptors).GenerateProxyType();
                }

                return _instanceFactories[type] = this.CreateInstanceFactory(proxyType);   
            }
        }

        public Func<InterceptorDecoration, IServiceProvider, object> GetTypeFactory(Type type, InterceptorDecoration interceptors)
        {
            if (_typeFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }

            lock (_sync)
            {
                if (_typeFactories.TryGetValue(type, out factory))
                {
                    return factory;
                }

                if (!_generatedClasses.TryGetValue(type, out var proxyType))
                {
                    proxyType = _generatedClasses[type] = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(type, interceptors).GenerateProxyType();
                }

                return _typeFactories[type] = this.CreateTypeFactory(proxyType);
            }
        }

        private Func<object, InterceptorDecoration, object> CreateInstanceFactory(Type proxyType)
        {
            var target = Expression.Parameter(typeof(object));
            var interceptors = Expression.Parameter(typeof(InterceptorDecoration));
            var typeToIntercept = Expression.Constant(proxyType);

            var constructor = proxyType.GetConstructors()[0];
            var targetType = constructor.GetParameters()[0].ParameterType;

            var convert = Expression.Convert(target, targetType);
            var create = Expression.New(constructor, convert, interceptors);
            return Expression.Lambda<Func<object, InterceptorDecoration, object>>(create, target, interceptors).Compile();
        }

        private Func<InterceptorDecoration, IServiceProvider, object> CreateTypeFactory(Type proxyType)
        {
            return (interceptors, serviceProvider) =>
            {
                var proxy = serviceProvider.GetService(proxyType);
                ((IInterceptorsInitializer)proxy).SetInterceptors(interceptors);
                return proxy;
            };
        }
    }
}
