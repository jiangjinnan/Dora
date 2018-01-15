namespace Dora.DynamicProxy
{
    /// <summary>
    /// Define a method to set the interceptor of type <see cref="InterceptorDecoration"/>.
    /// </summary>
    public interface IInterceptorsInitializer
    {
        /// <summary>
        /// Sets the interceptor of type <see cref="InterceptorDecoration"/>.
        /// </summary>
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the interceptors to set.</param>
        void SetInterceptors(InterceptorDecoration interceptors);
    }
}
