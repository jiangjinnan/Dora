using System;

namespace Dora.Interception
{
    /// <summary>
    /// Code generation based execution context.
    /// </summary>
    public class CodeGenerationContext
    {
        /// <summary>
        /// Gets implemented interface or base type of generated interceptable proxy class.
        /// </summary>
        public Type InterfaceOrBaseType { get; }

        /// <summary>
        /// Gets interface implementation type.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets interceptor registration specific to implementation type.
        /// </summary>
        public IInterceptorRegistry Interceptors { get; }

        /// <summary>
        /// Create a new <see cref="CodeGenerationContext"/>.
        /// </summary>
        /// <param name="baseType">Implemented interface or base type of generated interceptable proxy class.</param>
        /// <param name="interceptors">Interceptor registration specific to implementation type.</param>
        public CodeGenerationContext(Type baseType, IInterceptorRegistry interceptors )
        {
            InterfaceOrBaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            Interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));
        }

        /// <summary>
        /// Create a new <see cref="CodeGenerationContext"/>.
        /// </summary>
        /// <param name="interface">Implemented interface or base type of generated interceptable proxy class.</param>
        /// <param name="interceptors">Interceptor registration specific to implementation type.</param>
        /// <param name="targetType">Interface implementation type.</param>
        public CodeGenerationContext(Type @interface, Type targetType, IInterceptorRegistry interceptors)
        {
            InterfaceOrBaseType = @interface ?? throw new ArgumentNullException(nameof(@interface));
            Interceptors = interceptors ?? throw new ArgumentNullException(nameof(interceptors));
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        }
    }
}
