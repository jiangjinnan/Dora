using Dora;
using Dora.Interception.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Hosting
{
  /// <summary>
  /// Define some extension methods specific to <see cref="IWebHostBuilder"/>.
  /// </summary>
  public static class WebHostBuilderExtensions
  {
    /// <summary>
    /// Register the startup filter <see cref="InterceptionServiceProviderStartupFilter"/>.
    /// </summary>
    /// <param name="builder">The web host builder to which the <see cref="InterceptionServiceProviderStartupFilter"/> is registered.</param>
    /// <returns>The web host builder with the registration of <see cref="InterceptionServiceProviderStartupFilter"/>.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="builder"/> is null.</exception>
    public static IWebHostBuilder UseInterception(this IWebHostBuilder builder)
    {
      Guard.ArgumentNotNull(builder, nameof(builder));
      return builder.ConfigureServices(svcs => svcs.AddTransient<IStartupFilter, InterceptionServiceProviderStartupFilter>());
    }
  }
}
