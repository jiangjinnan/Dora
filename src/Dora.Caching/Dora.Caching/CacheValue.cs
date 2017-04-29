namespace Dora.Caching
{
  /// <summary>
  /// A object to indicate whether cache entry exists and to get the value.
  /// </summary>
  public class CacheValue
  {
    private readonly static CacheValue _nonExistent = new CacheValue();

    /// <summary>
    /// A <see cref="bool"/> value indicating whether the cache entry exists.
    /// </summary>
    /// <remarks>The existence cannot be determined by its value as the value can be explicitly specified as null.</remarks>
    public bool Exists { get; set; }

    /// <summary>
    /// The value of cache entry.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Check if the current state is valid.
    /// </summary>
    /// <remarks>It is invalid if <see cref="Exists"/> = false and <see cref="Value"/> != null.</remarks>
    public virtual bool IsValid
    {
      get { return this.Exists || null == this.Value; }
    }

    /// <summary>
    /// A <see cref="CacheValue"/> representing the cache entry does not exist.
    /// </summary>
    public static CacheValue NonExistent
    {
      get { return _nonExistent; }
    }
  }
}
