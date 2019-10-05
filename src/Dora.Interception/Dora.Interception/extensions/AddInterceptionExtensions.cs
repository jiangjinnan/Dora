using Dora;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AddInterceptionExtensions
    {
        #region AddInterceptable
        public static IServiceCollection AddInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.AlwaysAdd);
        public static IServiceCollection AddTransientInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType,  ServiceLifetime.Transient, AddKind.AlwaysAdd);
        public static IServiceCollection AddScopedInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Scoped, AddKind.AlwaysAdd);
        public static IServiceCollection AddSingletonInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Singleton, AddKind.AlwaysAdd);


        public static IServiceCollection AddInterceptable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplementation: TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), lifetime, AddKind.AlwaysAdd);
        public static IServiceCollection AddTransientInterceptable<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation),  ServiceLifetime.Transient, AddKind.AlwaysAdd);
        public static IServiceCollection AddScopedInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, AddKind.AlwaysAdd);
        public static IServiceCollection AddSingletonInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, AddKind.AlwaysAdd);
        #endregion

        #region TryAddInterceptable
        public static IServiceCollection TryAddInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.TryAdd);
        public static IServiceCollection TryAddTransientInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Transient, AddKind.TryAdd);
        public static IServiceCollection TryAddScopedInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Scoped, AddKind.TryAdd);
        public static IServiceCollection TryAddSingletonInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Singleton, AddKind.TryAdd);

        public static IServiceCollection TryAddInterceptable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), lifetime, AddKind.TryAdd);
        public static IServiceCollection TryAddInterceptable<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, AddKind.TryAdd);
        public static IServiceCollection TryAddScopedInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, AddKind.TryAdd);
        public static IServiceCollection TryAddSingletonInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, AddKind.TryAdd);
        #endregion

        #region TryAddEnumerableInterceptable
        public static IServiceCollection TryAddEnumerableInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.TryAddEnumerable);

        public static IServiceCollection TryAddEnumerableInterceptable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), lifetime, AddKind.TryAddEnumerable);
        #endregion

        private static ICodeGeneratorFactory _codeGeneratorFactory;
        private static IInterceptorResolver _interceptorResolver;
        private static IServiceCollection AddInterceptableCore(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime, AddKind kind)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            Guard.ArgumentNotNull(implementationType, nameof(implementationType));

            if (serviceType.IsGenericTypeDefinition)
            {
                if (!services.Any(it => it.ServiceType == typeof(ICodeGeneratorFactory)))
                {
                    throw new InvalidOperationException("IServiceCollection's AddInterception method must be called before register open generic service type.");
                }
                if (_codeGeneratorFactory == null)
                {
                    var provider = services.BuildServiceProvider();
                    _codeGeneratorFactory = provider.GetRequiredService<ICodeGeneratorFactory>();
                    _interceptorResolver = provider.GetRequiredService<IInterceptorResolver>();
                }
                if (serviceType != implementationType)
                {
                    services.AddTransient(implementationType, implementationType);
                }
            }
            switch (kind)
            {
                case AddKind.AlwaysAdd:
                    {
                        if (serviceType.IsGenericTypeDefinition)
                        {
                            services.Add(new GenericInterceptableServiceDescriptor(_codeGeneratorFactory, _interceptorResolver, serviceType, implementationType, lifetime));
                        }
                        else
                        {
                            services.Add(new InterceptableServiceDescriptor(serviceType, implementationType, lifetime));
                        }
                        break;
                    }
                case AddKind.TryAdd:
                    {
                        if (serviceType.IsGenericTypeDefinition)
                        {
                            services.TryAdd(new GenericInterceptableServiceDescriptor(_codeGeneratorFactory, _interceptorResolver, serviceType, implementationType, lifetime));
                        }
                        else
                        {
                            services.TryAdd(new InterceptableServiceDescriptor(serviceType, implementationType, lifetime));
                        }
                        break;
                    }
                case AddKind.TryAddEnumerable:
                    {
                        if (!services.OfType<IInterceptableServiceDescriptor>().Any(it => it.TargetType == implementationType))
                        {
                            if (serviceType.IsGenericTypeDefinition)
                            {
                                services.TryAddEnumerable(new GenericInterceptableServiceDescriptor(_codeGeneratorFactory, _interceptorResolver, serviceType, implementationType, lifetime));
                            }
                            else
                            {
                                services.TryAddEnumerable(new InterceptableServiceDescriptor(serviceType, implementationType, lifetime));
                            }
                        }
                        break;
                    }
            }
            return services;
        }

        private enum AddKind
        {
            AlwaysAdd,
            TryAdd,
            TryAddEnumerable
        }
    }
}
