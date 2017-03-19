using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Dora.Interception.AspNetCore
{
  /// <summary>
  /// A custom startup filter to register <see cref="InterceptionServiceProviderMiddleware"/>.
  /// </summary>
  public class InterceptionServiceProviderStartupFilter : IStartupFilter
  {
    /// <summary>
    /// Register <see cref="InterceptionServiceProviderMiddleware"/>.
    /// </summary>
    /// <param name="next">A delegate to perform subsequent configuration.</param>
    /// <returns>The delegate to configure the application.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="next"/> is null.</exception>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
      Guard.ArgumentNotNull(next, nameof(next));
      return builder =>
      {
        builder.UseMiddleware<InterceptionServiceProviderMiddleware>();
        next(builder);
      };
    }
  }
}
