namespace Dora.Interception
{
    /// <summary>
    /// A facade class used to get current <see cref="IMethodInvokerBuilder"/>.
    /// </summary>
    public static class MethodInvokerBuilder
    {
        /// <summary>
        /// Gets the current <see cref="IMethodInvokerBuilder"/>..
        /// </summary>
        /// <value>
        /// The current <see cref="IMethodInvokerBuilder"/>.
        /// </value>
        public static IMethodInvokerBuilder Instance { get; internal set; } = default!;
    }
}
