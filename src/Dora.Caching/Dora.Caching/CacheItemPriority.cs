namespace Dora.Caching
{
  /// <summary>
  /// Specifies how items are prioritized for preservation during a memory pressure triggered cleanup.
  /// </summary>
  public enum CacheItemPriority
  {
    /// <summary>
    /// Low
    /// </summary>
    Low,

    /// <summary>
    /// Normal
    /// </summary>
    Normal,

    /// <summary>
    /// High
    /// </summary>
    High,
    /// <summary>
    /// High
    /// </summary>
    NeverRemove
  }
}
