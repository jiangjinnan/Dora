using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpContextExtensions
    {
        private const string KeyofExceptionInfo = "Dora.ExceptionHandling.Mvc.ExceptionInfo";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exceptionInfo"></param>
        /// <returns></returns>
        public static HttpContext SetExceptionInfo(this HttpContext context, ExceptionInfo exceptionInfo)
        {
            Guard.ArgumentNotNull(context, nameof(context)).Items[KeyofExceptionInfo] = Guard.ArgumentNotNull(exceptionInfo, nameof(exceptionInfo));
            return context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exceptionInfo"></param>
        /// <returns></returns>
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
