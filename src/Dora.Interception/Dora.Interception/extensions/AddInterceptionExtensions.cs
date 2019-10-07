using Dora;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines <see cref="IServiceCollection"/> specific extension methods.
    /// </summary>
    public static class AddInterceptionExtensions
    {
        #region AddInterceptable

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with transient lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddTransientInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType,  ServiceLifetime.Transient, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with scoped lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddScopedInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Scoped, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with singleton lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddSingletonInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Singleton, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddInterceptable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplementation: TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), lifetime, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with transient lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddTransientInterceptable<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation),  ServiceLifetime.Transient, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with scoped lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddScopedInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, AddKind.AlwaysAdd);

        /// <summary>
        /// Add interceptable <see cref="ServiceDescriptor"/> with singleton lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection AddSingletonInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, AddKind.AlwaysAdd);
        #endregion

        #region TryAddInterceptable

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with transient lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddTransientInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Transient, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with scoped lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddScopedInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Scoped, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with singleton lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddSingletonInterceptable(this IServiceCollection services, Type serviceType, Type implementationType)
            => services.AddInterceptableCore(serviceType, implementationType, ServiceLifetime.Singleton, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddInterceptable<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), lifetime, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with transient lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddInterceptable<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : TService
            => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with scoped lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddScopedInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped, AddKind.TryAdd);

        /// <summary>
        /// Try to add interceptable <see cref="ServiceDescriptor"/> with singleton lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddSingletonInterceptable<TService, TImplementation>(this IServiceCollection services)
           where TImplementation : TService
           => services.AddInterceptableCore(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, AddKind.TryAdd);
        #endregion

        #region TryAddEnumerableInterceptable

        /// <summary>
        /// Adds a interceptable <see cref="ServiceDescriptor"/> if an existing one with the same type and implementation that does not already exist in services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="serviceType">The type of service registration.</param>
        /// <param name="implementationType">The implementation type of service registration.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
        public static IServiceCollection TryAddEnumerableInterceptable(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
            => services.AddInterceptableCore(serviceType, implementationType, lifetime, AddKind.TryAddEnumerable);

        /// <summary>
        /// Adds a interceptable <see cref="ServiceDescriptor"/> if an existing one with the same type and implementation that does not already exist in services.
        /// </summary>
        /// <typeparam name="TService">The type of service registration.</typeparam>
        /// <typeparam name="TImplementation">The implementation type of service registration.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> which the new service registration is added in.</param>
        /// <param name="lifetime">The lifetime of service registration.</param>
        /// <returns>The current <see cref="IServiceCollection"/></returns>.
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
