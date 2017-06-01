using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// Type matching based excpetion filter.
    /// </summary>
    public class MatchTypeFilter : IExceptionFilter
    {
        /// <summary>
        /// The exception type to match.
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// Creates a new <see cref="MatchTypeFilter"/>.
        /// </summary>
        /// <param name="exceptionType">The exception type to match.</param>
        /// <exception cref="ArgumentNullException"> The specified <paramref name="exceptionType"/> is null.</exception>
        public MatchTypeFilter(Type exceptionType)
        {
            this.ExceptionType = Guard.ArgumentAssignableTo<Exception>(exceptionType, nameof(exceptionType));
        }

        /// <summary>
        /// Determines whether to match the specified <see cref="ExceptionContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ExceptionContext"/> to match.</param>
        /// <returns>Indicates whether to match the specified <see cref="ExceptionContext"/>.</returns>
        /// <exception cref="ArgumentNullException"> The specified <paramref name="context"/> is null.</exception>
        public bool Match(ExceptionContext context)
        {
            return this.ExceptionType.GetTypeInfo().IsAssignableFrom(Guard.ArgumentNotNull(context, nameof(context)).Exception.GetType());
        }
    }
}
