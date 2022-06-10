namespace Dora.Interception
{
    /// <summary>
    /// Factory to create interceptor based on specified type and optional arguments.
    /// </summary>
    public interface IConventionalInterceptorFactory
    {
        /// <summary>
        /// Creates the interceptor based <see cref="InvokeDelegate"/>.
        /// </summary>
        /// <param name="interceptorType">Type of the interceptor based <see cref="InvokeDelegate"/>.</param>
        /// <param name="arguments">The arguments passed to constructor.</param>
        /// <returns>The created interceptor based <see cref="InvokeDelegate"/>.</returns>
        InvokeDelegate CreateInterceptor(Type interceptorType, params object[] arguments);
    }
}
