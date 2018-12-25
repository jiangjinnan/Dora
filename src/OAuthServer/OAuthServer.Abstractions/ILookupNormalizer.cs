namespace Dora.OAuthServer
{
    /// <summary>
    /// Provides functionaliy to normalize the case-insensitive indentifier.
    /// </summary>
    public interface ILookupNormalizer
    {
        /// <summary>
        /// Normalizes the case-insensitive indentifier.
        /// </summary>
        /// <param name="identifier">The raw identifier to normalize.</param>
        /// <returns>The normalized identifier.</returns>
        string Normalize(string identifier);
    }
}