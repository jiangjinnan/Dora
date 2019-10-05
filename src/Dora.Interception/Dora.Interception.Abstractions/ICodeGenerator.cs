using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents generator used to generate interceptable proxy class.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates interceptable proxy class.
        /// </summary>
        /// <param name="context">The <see cref="CodeGenerationContext"/> representing code generation based execution context.</param>
        /// <returns>The generated interceptable proxy class</returns>
        Type GenerateInterceptableProxyClass(CodeGenerationContext  context);
    }
}