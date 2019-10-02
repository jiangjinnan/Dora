namespace Dora.Interception
{
    /// <summary>
    /// Represents factory to create <see cref="ICodeGenerator"/>.
    /// </summary>
    public interface ICodeGeneratorFactory
    {
        /// <summary>
        /// Creates a new <see cref="ICodeGenerator"/> used to generate interceptable proxy class.
        /// </summary>
        /// <returns>Created <see cref="ICodeGenerator"/> used to generate interceptable proxy class..</returns>
        ICodeGenerator Create();
    }
}
