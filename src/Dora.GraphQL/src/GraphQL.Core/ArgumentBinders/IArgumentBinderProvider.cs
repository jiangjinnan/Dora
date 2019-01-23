namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Represents the argument binder provider.
    /// </summary>
    public interface IArgumentBinderProvider
    {
        /// <summary>
        /// Gets the argument binder.
        /// </summary>
        /// <returns>The <see cref="IArgumentBinder"/>.</returns>
        IArgumentBinder GetArgumentBinder();
    }
}
