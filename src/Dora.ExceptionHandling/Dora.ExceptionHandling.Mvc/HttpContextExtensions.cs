using Microsoft.AspNetCore.Http;
using System;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// Defines extension methods to attache <see cref="ExceptionInfo"/> to the specified <see cref="HttpContext"/> and to get attached <see cref="ExceptionInfo"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        private const string KeyofExceptionInfo = "Dora.ExceptionHandling.Mvc.ExceptionInfo";

        /// <summary>
        /// Attach an <see cref="ExceptionInfo"/> to specified <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> to which the specified <see cref="ExceptionInfo"/> is attached.</param>
        /// <param name="exceptionInfo">The <see cref="ExceptionInfo"/> to attach.</param>
        /// <returns>The specified <see cref="HttpContext"/> with attached <see cref="ExceptionInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="exceptionInfo"/> is null.</exception>
        public static HttpContext SetExceptionInfo(this HttpContext context, ExceptionInfo exceptionInfo)
        {
            Guard.ArgumentNotNull(context, nameof(context)).Items[KeyofExceptionInfo] = Guard.ArgumentNotNull(exceptionInfo, nameof(exceptionInfo));
            return context;
        }

        /// <summary>
        /// Try to get attached <see cref="ExceptionInfo"/> from the specified <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> from which the <see cref="ExceptionInfo"/> is loaded.</param>
        /// <param name="exceptionInfo">The <see cref="ExceptionInfo"/> to get.</param>
        /// <returns>Indicates whether to successfully get the attached <see cref="ExceptionInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="context"/> is null.</exception>
        public static bool TryGetExceptionInfo(this HttpContext context, out ExceptionInfo exceptionInfo)
        {
            if (Guard.ArgumentNotNull(context, nameof(context)).Items.TryGetValue(KeyofExceptionInfo, out object value))
            {
                return (exceptionInfo = value as ExceptionInfo) != null;
            }
            return (exceptionInfo = null) != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpContext context)
        {
            return Guard.ArgumentNotNull(context, nameof(context)).Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
