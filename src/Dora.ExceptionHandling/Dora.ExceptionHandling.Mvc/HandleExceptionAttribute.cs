using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Filters = Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// A custom exception filter to leverage registered <see cref="ExceptionManager"/> to handle the exception.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method)]
    public class HandleExceptionAttribute : Filters.ExceptionFilterAttribute
    {
        private HandleExceptionFilterOptions _options;

        /// <summary>
        /// The name of exception policy to handle the exception.
        /// </summary>
        public string ExceptionPolicy { get; set; }

        /// <summary>
        /// Handle exception.
        /// </summary>
        /// <param name="context">The cu<see cref="Filters.ExceptionContext"/>.</param>
        /// <returns>The task to handle exception.</returns>
        public override async Task OnExceptionAsync(Filters.ExceptionContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            context.ExceptionHandled = true;
            _options = context.HttpContext.RequestServices.GetRequiredService<IOptions<HandleExceptionFilterOptions>>().Value;
            if (context.Exception == null)
            {
                return;
            }

            ExceptionManager manager = context.HttpContext.RequestServices.GetRequiredService<ExceptionManager>();
            string exceptionPolicy = this.GetExceptionPolicy(context);
            try
            {
                if (!string.IsNullOrEmpty(exceptionPolicy))
                {
                    await manager.HandleExceptionAsync(context.Exception, exceptionPolicy);
                }
                else
                {
                    manager.HandleExceptionAsync(context.Exception).Wait();
                }
            }
            catch (Exception ex)
            {
                ex = (ex as AggregateException)?.InnerException??ex;
                var exceptionInfo = new ExceptionInfo(ex, _options.IncludeInnerException);
                if (this.TryGetHandlerAction(context, out ActionDescriptor handlerAction))
                {
                    context.HttpContext.SetExceptionInfo(exceptionInfo);
                    ActionContext actionContext = new ActionContext(context.HttpContext, context.RouteData, handlerAction);
                    IActionInvoker actionInvoker = context.HttpContext.RequestServices.GetRequiredService<IActionInvokerFactory>().CreateInvoker(actionContext);
                    await actionInvoker.InvokeAsync();
                    return;
                }

                if (context.HttpContext.IsAjaxRequest())
                {
                    JsonResult json = new JsonResult(exceptionInfo, _options.JsonSerializerSettings);
                    await json.ExecuteResultAsync(new ActionContext(context));
                    return;
                }
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
                {
                    Model = exceptionInfo
                };
                ViewResult view = new ViewResult { ViewData = viewData };
                await view.ExecuteResultAsync(context);
            }
        }
        /// <summary>
        /// Get the name of exception policy to handle the exception.
        /// </summary>
        /// <param name="context">The current <see cref="ExceptionContext"/>.</param>
        /// <returns>The name of exception policy to handle the exception.</returns>
        protected virtual string GetExceptionPolicy(Filters.ExceptionContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            if (!string.IsNullOrEmpty(this.ExceptionPolicy))
            {
                return this.ExceptionPolicy;
            }

            ControllerActionDescriptor actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (null != actionDescriptor)
            {
               var attribute = actionDescriptor.MethodInfo.GetCustomAttribute<ExceptionPolicyAttribute>()
                    ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<ExceptionPolicyAttribute>();
                if (null != attribute)
                {
                    return attribute.PolicyName;
                }
            }
            return _options.ExceptionPolicy;
        }

        /// <summary>
        /// Try to get the exception handler action to process the request.
        /// </summary>
        /// <param name="context">The current <see cref="ExceptionContext"/>.</param>
        /// <param name="handlerAction">The <see cref="ActionDescriptor"/> representing the exception handler action.</param>
        /// <returns>Indicates whether the current action specific handler action exists.</returns>
        protected virtual bool TryGetHandlerAction(Filters.ExceptionContext context, out ActionDescriptor handlerAction)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            ControllerActionDescriptor actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (null != actionDescriptor)
            {
                var attribute = actionDescriptor.MethodInfo.GetCustomAttribute<HandlerActionAttribute>()
                     ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<HandlerActionAttribute>();
                string handlerActionName = attribute?.HandlerAction ?? _options.ResolveHandlerAction(actionDescriptor.ActionName);
                var provider = context.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
                var result = from it in provider.ActionDescriptors.Items
                             let it2 = it as ControllerActionDescriptor
                             where it2 != null && it2.ActionName == handlerActionName && it2.ControllerTypeInfo == actionDescriptor.ControllerTypeInfo
                             select it;
                return (handlerAction = result.FirstOrDefault()) != null;
            }
            return (handlerAction = null) != null;
        }
    }
}
