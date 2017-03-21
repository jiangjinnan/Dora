using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  public class CacheEntryOptions
  {
    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public CacheItemPriority Priority { get; set; }
    public Func<IEnumerable<IChangeToken>> ExpirationTokensAccessor { get; set; } = new Func<IEnumerable<IChangeToken>>(() => new IChangeToken[0]);
  }
}
