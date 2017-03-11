namespace Dora.Interception
{
    /// <summary>
    /// Represents an interceptor.
    /// </summary>
    /// <param name="next">A <see cref="InterceptDelegate"/> used to invoke the next interceptor or target method.</param>
    /// <returns>A <see cref="InterceptDelegate"/> representing the interception operation.</returns>
    public delegate InterceptDelegate InterceptorDelegate(InterceptDelegate next);
}
