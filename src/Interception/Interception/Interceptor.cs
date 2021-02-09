using System;

namespace Dora.Interception
{
    /// <summary>
    /// Interceptor.
    /// </summary>
    public sealed class Interceptor : IInterceptor
    {
        /// <summary>
        /// Gets the delegate to execute interception operation.
        /// </summary>
        /// <value>
        /// The  delegate to execute interception operation.
        /// </value>
        public InterceptorDelegate Delegate { get; }

        /// <summary>
        /// Gets a value indicating whether [capture arguments].
        /// </summary>
        /// <value>
        /// <c>true</c> if [capture arguments]; otherwise, <c>false</c>.
        /// </value>
        public bool CaptureArguments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Interceptor"/> class.
        /// </summary>
        /// <param name="delegate">The delegate to execute interception operation.</param>
        /// <param name="captureArguments">if set to <c>true</c> [capture arguments].</param>
        public Interceptor(InterceptorDelegate @delegate, bool captureArguments)
        {
            Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            CaptureArguments = captureArguments;
        }
    }
}
