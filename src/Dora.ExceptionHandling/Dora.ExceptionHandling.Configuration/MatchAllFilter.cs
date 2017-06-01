namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// The exception filter which matches all kinds of <see cref="ExceptionContext"/>, and it means the specific exception handler is always invoked.
    /// </summary>
    public class MatchAllFilter : IExceptionFilter
    {
        /// <summary>
        /// Determines whether to match the specified <see cref="ExceptionContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ExceptionContext"/> to match.</param>
        /// <returns>Indicates whether to match the specified <see cref="ExceptionContext"/>.</returns>
        public bool Match(ExceptionContext context)
        {
            return true;
        }
    }
}
