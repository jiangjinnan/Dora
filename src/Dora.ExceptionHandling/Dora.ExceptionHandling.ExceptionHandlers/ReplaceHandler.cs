using Dora.ExceptionHandling.Abstractions.Properties;
using Dora.ExceptionHandling.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// An exception handler to replace the current exception with specified type of exception.
    /// </summary>
    [HandlerConfiguration(typeof(ReplaceHandlerConfiguration))]
    public class ReplaceHandler
    {
        /// <summary>
        /// The type of exception to replace the current exception.
        /// </summary>
        public Type ReplaceExcecptionType { get; }

        /// <summary>
        /// The message of the exception to replace the current exception.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create a new <see cref="ReplaceHandler"/>.
        /// </summary>
        /// <param name="replaceExcecptionType">The type of exception to replace the current exception.</param>
        /// <param name="message">The message of the exception to replace the current exception.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="replaceExcecptionType"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="replaceExcecptionType"/> is not an exception type.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="message"/> is a white space string.</exception>
        public ReplaceHandler(Type replaceExcecptionType, string message)
        {
            this.ReplaceExcecptionType = Guard.ArgumentAssignableTo<Exception>(replaceExcecptionType, nameof(replaceExcecptionType));
            if (replaceExcecptionType.GetTypeInfo().GetConstructor(new Type[] { typeof(string) }) == null)
            {
                throw new ArgumentException(Resources.ExceptionInvalidReplaceExceptionType.Fill(replaceExcecptionType.FullName), nameof(replaceExcecptionType));
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
            context.Exception = (Exception)Activator.CreateInstance(this.ReplaceExcecptionType, this.Message);
            return Task.CompletedTask;
        }
    }
}
