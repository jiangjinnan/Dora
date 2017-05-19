using System;

namespace Dora.ExceptionHandling.Mvc
{
    public class HandleExceptionAttribute : Microsoft.AspNetCore.Mvc.Filters.IExceptionFilter
    {
        public void OnException(Microsoft.AspNetCore.Mvc.Filters.ExceptionContext context)
        {
            //Guard.ArgumentNotNull(context, nameof(context));
            //if (context.Exception == null)
            //{
            //    return;
            //}
            //ExceptionManager
            //try
            //{

            //}
        }
    }
}
