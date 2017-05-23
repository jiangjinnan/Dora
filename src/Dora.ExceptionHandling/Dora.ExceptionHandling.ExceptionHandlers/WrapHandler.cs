using Dora.ExceptionHandling.Abstractions.Properties;
using Dora.ExceptionHandling.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// An exception handler to wrap the current exception with specified type of exception.
    /// </summary>
    [HandlerConfiguration(typeof(WrapHandlerConfiguration))]
    public class WrapHandler
    {
        /// <summary>
        /// The type of exception to Wrap the current exception.
        /// </summary>
        public Type WrapExcecptionType { get; }

        /// <summary>
        /// The message of the exception to Wrap the current exception.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create a new <see cref="WrapHandler"/>.
        /// </summary>
        /// <param name="wrapExcecptionType">The type of exception to wrap the current exception.</param>
        /// <param name="message">The message of the exception to wrap the current exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="wrapExcecptionType"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="wrapExcecptionType"/> is not an exception type.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="message"/> is a white space string.</exception>
        public WrapHandler(Type wrapExcecptionType, string message)
        {
            this.WrapExcecptionType = Guard.ArgumentAssignableTo<Exception>(wrapExcecptionType, nameof(wrapExcecptionType));
            if (wrapExcecptionType.GetTypeInfo().GetConstructor(new Type[] { typeof(string), typeof(Exception) }) == null)
            {
                throw new ArgumentException(Resources.ExceptionInvalidWrapExceptionType.Fill(wrapExcecptionType.FullName), nameof(wrapExcecptionType));
            }
            this.Message = Guard.ArgumentNotNullOrWhiteSpace(message, nameof(message));
        }

        /// <summary>
        /// Handle the exception.
        /// </summary>
        /// <param name="context">The exception hanlding execution context.</param>
        /// <returns>The task to handle the exception.</returns>
        public Task HandleExceptionAsync(ExceptionContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            context.Exception = (Exception)Activator.CreateInstance(this.WrapExcecptionType, this.Message, context.Exception);
            return Task.CompletedTask;
        }
    }
}
