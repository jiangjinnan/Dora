using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Dora.Interception
{
    public static partial class InterceptionBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="InterceptorFilter"/> to register interceptors.
        /// </summary>
        /// <param name="builder">The <see cref="InterceptionBuilder"/> used to perform interception based service registration.</param>
        /// <param name="configure">The <see cref="Action{InterceptionFilterBuilder}"/> to configure the <see cref="InterceptorFilter"/> to build.</param>
        /// <returns>The current <see cref="InterceptionBuilder"/>.</returns>
        public static InterceptionBuilder AddFilters(this InterceptionBuilder builder, Action<InterceptionFilterBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configure, nameof(configure));
            var filterBuilder = new InterceptionFilterBuilder();
            configure(filterBuilder);
            builder.InterceptorProviderResolvers.Add(nameof(InterceptorFilter),filterBuilder.Build());
            return builder;
        }
    }

    /// <summary>
    /// Represents builder to build <see cref="InterceptorFilter"/>.
    /// </summary>
    public class InterceptionFilterBuilder
    {
        private readonly InterceptorFilter _filter = new InterceptorFilter();

        /// <summary>
        /// Builds the <see cref="InterceptorFilter"/>.
        /// </summary>
        /// <returns>The built <see cref="InterceptorFilter"/>.</returns>
        public InterceptorFilter Build() => _filter;

        /// <summary>
        /// Adds the specified interceptor provider with filter.
        /// </summary>
        /// <param name="interceptorProvider">The <see cref="IInterceptorProvider"/> to register.</param>
        /// <param name="predicate">The <see cref="Func{MethodInfo, Boolean}"/> determining whether the specified <see cref="IInterceptorProvider"/> should applied to specified method.</param>
        /// <returns>The current <see cref="InterceptionFilterBuilder"/>.</returns>
        public InterceptionFilterBuilder Add(IInterceptorProvider interceptorProvider, Func<MethodInfo, bool> predicate)
        {
            _filter.Add(interceptorProvider, predicate);
            return this;
        }
    }
}