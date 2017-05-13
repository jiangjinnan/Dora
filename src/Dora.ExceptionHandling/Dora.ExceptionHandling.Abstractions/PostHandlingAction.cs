namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Determining what action should occur after an exception is handled by the configured exception handling chain.
    /// </summary>
    public enum PostHandlingAction
    {
        /// <summary>
        /// Indicates that no rethrow should occur.
        /// </summary>
        None,

        /// <summary>
        /// Throw original exception.
        /// </summary>
        ThrowOriginal,

        /// <summary>
        /// The the new exception which is used by a particular exception handler to wrap or replace the original exception.
        /// </summary>
        ThrowNew
    }
}
