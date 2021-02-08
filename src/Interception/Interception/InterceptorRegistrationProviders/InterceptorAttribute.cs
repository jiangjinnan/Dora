using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method)]
    public class InterceptorAttribute: Attribute
    {
        private readonly Type _interceptorType;
        private readonly object[] _arguments;

        public int Order { get; set; }

        public InterceptorAttribute(Type interceptorType, params object[] arguments)
        {
            _interceptorType = interceptorType ?? throw new ArgumentNullException(nameof(interceptorType));
            _arguments = arguments;
        }

        public InterceptorAttribute()
        { }

        internal protected virtual object CreateInterceptor(IServiceProvider serviceProvider)
        {
            if (_interceptorType == null)
            {
                throw new InvalidOperationException("Interceptor type is not specified, and the CreateInterceptor method should be overriden.");
            }
            return ActivatorUtilities.CreateInstance(serviceProvider, _interceptorType, _arguments??Array.Empty<object>());
        }

        protected T CreateInterceptor<T>(IServiceProvider serviceProvider, params object[] arguments)
        {
            return ActivatorUtilities.CreateInstance<T>(serviceProvider, arguments ?? Array.Empty<object>());
        }
    }
}
