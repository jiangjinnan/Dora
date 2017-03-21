using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  /// <summary>
  /// The cache entry specific options.
  /// </summary>
  public class CacheEntryOptions
  {
    /// <summary>
    /// Gets or sets an absolute expiration date for the cache entry.
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets an absolute expiration time, relative to now.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set).
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the priority for keeping the cache entry in the cache during a memory pressure triggered cleanup. The fefault value is <see cref="CacheItemPriority.Normal"/>.
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    /// <summary>
    /// Gets the <see cref="IChangeToken"/> instances which cause all cache entries to expire.
    /// </summary>
    public Func<IChangeToken> ExpirationTokenAccessor { get; set; }
  }
}
