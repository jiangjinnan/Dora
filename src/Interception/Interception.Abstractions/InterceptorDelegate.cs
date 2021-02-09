namespace Dora.Interception
{
    /// <summary>
    /// Interceptor delegate.
    /// </summary>
    /// <param name="next">The subsequential interceptor chain.</param>
    /// <returns>The new interceptor chain including current interceptor.</returns>
    public delegate InvokerDelegate InterceptorDelegate(InvokerDelegate next);
}
