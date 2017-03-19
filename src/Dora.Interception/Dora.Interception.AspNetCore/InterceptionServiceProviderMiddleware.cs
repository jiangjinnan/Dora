using Dora.Interception.AspNetCore.Properties;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Dora.Interception.AspNetCore
{
  /// <summary>
  /// A middleware to register the <see cref="InterceptableServiceProvider"/>as the request specific service provider.
  /// </summary>
  public class InterceptionServiceProviderMiddleware
  {
    private readonly RequestDelegate _next;

    /// <summary>
    /// Create a new <see cref="InterceptionServiceProviderMiddleware"/>.
    /// </summary>
    /// <param name="next">A <see cref="RequestDelegate"/> representing the subsequent middleware pileline.</param>
    /// <exception cref="ArgumentNullException">The argument <paramref name="next"/> is null.</exception>
    public InterceptionServiceProviderMiddleware(RequestDelegate next)
    {
      Guard.ArgumentNotNull(next, nameof(next));
      _next = next;
    }

    /// <summary>
    /// Create a new <see cref="InterceptableServiceProvider"/> to override the current request specific service provider.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> representing the current request context.</param>
    /// <returns>A task to register <see cref="InterceptableServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The current request specific service provider is not initialized.</exception>
    public async Task Invoke(HttpContext context)
    {
      Guard.ArgumentNotNull(context, nameof(context));
      if (null == context.RequestServices)
      {
        throw new InvalidOperationException(Resources.ExceptionRequestServicesNotInitialized);
      }
      IServiceProvider original = context.RequestServices;
      try
      {
        context.RequestServices = new InterceptableServiceProvider(context.RequestServices);
        await _next(context);
      }
      finally
      {
        context.RequestServices = original;
      }
    }
  }
}
