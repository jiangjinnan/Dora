namespace Dora.Interception
{
    /// <summary>
    /// Accessor to get application based <see cref="IServiceProvider"/>.
    /// </summary>
    public interface IApplicationServicesAccessor
    {
        /// <summary>
        /// Gets the application based <see cref="IServiceProvider"/>.
        /// </summary>
        /// <value>
        /// The application based <see cref="IServiceProvider"/>.
        /// </value>
        IServiceProvider ApplicationServices { get; }
    }
}
