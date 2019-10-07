using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents open-generic-type based interceptable <see cref="ServiceDescriptor"/>.
    /// </summary>
    public sealed class GenericInterceptableServiceDescriptor : ServiceDescriptor, IInterceptableServiceDescriptor
    {
        private readonly Type _targetType;

        /// <summary>
        /// Create a new <see cref="GenericInterceptableServiceDescriptor"/>.
        /// </summary>
        /// <param name="codeGeneratorFactory">The <see cref="ICodeGeneratorFactory"/> used to get <see cref="ICodeGenerator"/>.</param>
        /// <param name="interceptorResolver">The <see cref="IInterceptorResolver"/> used to resolve registered interceptors.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The <see cref="Type"/> service implements.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service registration.</param>
        public GenericInterceptableServiceDescriptor(
            ICodeGeneratorFactory codeGeneratorFactory,
            IInterceptorResolver interceptorResolver,
            Type serviceType, Type implementationType, ServiceLifetime lifetime)
           : base(serviceType, GetInterceptableProxyType(codeGeneratorFactory, interceptorResolver, serviceType, implementationType), lifetime)
        {
            if (!serviceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Non-open-generic type (generic type definition) is not support", nameof(serviceType));
            }
            _targetType = implementationType;
        }

        Type IInterceptableServiceDescriptor.TargetType => _targetType;

        private static Type GetInterceptableProxyType(
            ICodeGeneratorFactory codeGeneratorFactory,
            IInterceptorResolver interceptorResolver,
            Type serviceType,
            Type implementationType)
        {
            var interceptors = serviceType.IsInterface
                ? interceptorResolver.GetInterceptors(serviceType, implementationType)
                : interceptorResolver.GetInterceptors(implementationType);
            var codeGenerator = codeGeneratorFactory.Create();
            var context = new CodeGenerationContext(serviceType, implementationType, interceptors);
            var proxyType = codeGenerator.GenerateInterceptableProxyClass(context);
            return proxyType;
        }
    }
}
