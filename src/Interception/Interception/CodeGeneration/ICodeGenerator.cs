using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception.CodeGeneration
{
    /// <summary>
    /// Code generator to write interceptable class source code.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Tries to generate source code of interceptable proxy class.
        /// </summary>
        /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor"/> representing the raw service registration.</param>
        /// <param name="codeGenerationContext">The code generation context.</param>
        /// <param name="proxyTypeNames">The names of generated classes.</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether generate class.</returns>
        bool TryGenerate(ServiceDescriptor serviceDescriptor, CodeGenerationContext codeGenerationContext, out string[]? proxyTypeNames);

        /// <summary>
        /// Registers the dynamically generated proxy types.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> holding all service registrations.</param>
        /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor"/> representing original service registration.</param>
        /// <param name="proxyTypes">The dynamically generated interceptable proxy types.</param>
        void RegisterProxyType(IServiceCollection services, ServiceDescriptor serviceDescriptor, Type[] proxyTypes);
    }
}
