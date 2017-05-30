using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// A data class to carry exception based information.
    /// </summary>
    [ModelBinder(BinderType = typeof(ExcecptionInfoBinder))]
    public class ExceptionInfo
    {
        /// <summary>
        /// The exception type's assembly qualified name.
        /// </summary>
        public string ExceptionType { get; }

        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        public string HelpLink { get; }

        /// <summary>
        /// Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
        /// </summary>
        public int HResult { get;  }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        public string Source { get;  }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        ///  Gets the <see cref="ExceptionInfo"/> that caused the current exception.
        /// </summary>
        public ExceptionInfo InnerException { get; }

        /// <summary>
        /// Creates a new <see cref="ExceptionInfo"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> which the <see cref="ExceptionInfo"/> is created based on.</param>
        /// <param name="includeInnerException">Indicates whether to include the inner exception.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="exception"/> is null.</exception>
        public ExceptionInfo(Exception exception, bool includeInnerException)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            this.ExceptionType = exception.GetType().AssemblyQualifiedName;
            this.HelpLink = exception.HelpLink;
            this.HResult = exception.HResult;
            this.Message = exception.Message;
            this.Source = exception.Source;
            this.StackTrace = exception.StackTrace;

            if (includeInnerException && exception.InnerException != null)
            {
                this.InnerException = new ExceptionInfo(exception.InnerException, true);
            }
        }
    }
}
