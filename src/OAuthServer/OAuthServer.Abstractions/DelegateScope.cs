namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents an OAuth scope.
    /// </summary>
    public class DelegateScope
    {
        #region Properties
        /// <summary>
        /// Identifier of the scope.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Title of the scope displayed in the delegate conset page.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Icon url of the scope displayed in the delegate conset page.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Description the scope displayed in the delegate conset page.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Indicates whether this scope is a default one, which is used when the scope is not explicitly provided.
        /// </summary>
        public bool IsDefault { get; }
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateScope"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <param name="iconUrl">The icon URL.</param>
        public DelegateScope(string id, string title, string description, bool isDefault, string iconUrl = null)
        {
            Id = Guard.ArgumentNotNullOrWhiteSpace(id, nameof(id));
            Title = Guard.ArgumentNotNullOrWhiteSpace(title, nameof(title));
            Description = Guard.ArgumentNotNullOrWhiteSpace(description, nameof(description));
            IsDefault = isDefault;
            IconUrl = iconUrl;
        }
    }
}
