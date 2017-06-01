namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// Represents a fitler to determine whether the registered exception handler should be invoked.
    /// </summary>
    public interface IExceptionFilter
    {
        /// <summary>
        /// Determines whether to match the specified <see cref="ExceptionContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ExceptionContext"/> to match.</param>
        /// <returns>Indicates whether to match the specified <see cref="ExceptionContext"/>.</returns>
        bool Match(ExceptionContext context);
    }
}
