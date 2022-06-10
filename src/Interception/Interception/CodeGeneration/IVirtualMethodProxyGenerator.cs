using System.Reflection;

namespace Dora.Interception.CodeGeneration
{
    /// <summary>
    /// Virtual method based code generator.
    /// </summary>
    interface IVirtualMethodProxyGenerator
    {
        /// <summary>
        /// Generates the interceptable proxy class.
        /// </summary>
        /// <param name="codeGenerationContext">The code generation context.</param>
        /// <param name="baseType">The base type.</param>
        /// <param name="interceptableMethods">The interceptable methods.</param>
        /// <returns>The source code of interceptable proxy class.</returns>
        string Generate(CodeGenerationContext codeGenerationContext, Type baseType, MethodInfo[] interceptableMethods);
    }
}
