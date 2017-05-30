using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// The <see cref="ExceptionInfo"/> specific model binder.
    /// </summary>
    public class ExcecptionInfoBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a model.
        /// </summary>
        /// <param name="bindingContext">The <see cref="ModelBindingContext"/> in which the model binding is performed.</param>
        /// <returns>The task to perform model binding.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (Guard.ArgumentNotNull(bindingContext, nameof(bindingContext)).ModelType == typeof(ExceptionInfo))
            {
               var model = bindingContext.HttpContext.TryGetExceptionInfo(out ExceptionInfo exceptionInfo)
                    ? exceptionInfo
                    : null;
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            return Task.CompletedTask;
        }
    }
}