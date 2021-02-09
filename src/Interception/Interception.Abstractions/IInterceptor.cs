namespace Dora.Interception
{
    /// <summary>
    /// Interceptor.
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// Gets a value indicating whether [capture arguments].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [capture arguments]; otherwise, <c>false</c>.
        /// </value>
        bool CaptureArguments { get; }

        /// <summary>
        /// Gets the delegate to execute interception operation.
        /// </summary>
        /// <value>
        /// The  delegate to execute interception operation.
        /// </value>
        InterceptorDelegate Delegate { get; }
    }
}
