namespace Dora.OAuthServer
{
    public class LookupNormalizer : ILookupNormalizer
    {
        public string Normalize(string identifier)
        {
            Guard.ArgumentNotNullOrWhiteSpace(identifier, nameof(identifier));
            return identifier.ToLowerInvariant();
        }
    }
}
