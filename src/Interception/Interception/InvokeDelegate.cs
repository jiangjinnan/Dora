namespace Dora.Interception
{
    /// <summary>
    /// Method invocation delegate.
    /// </summary>
    /// <param name="context">The method invocation context.</param>
    /// <returns>The <see cref="ValueTask"/> to perpform method invocation.</returns>
    public delegate ValueTask InvokeDelegate(InvocationContext context);
}
