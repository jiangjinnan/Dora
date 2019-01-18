using System;
using System.Linq;

namespace Dora.OAuthServer
{
    /// <summary>
    /// <see cref="Application"/> specific EF entity class.
    /// </summary>
    public class ApplicationEntity
    {
        /// <summary>
        /// Client identifier of the registered application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Display name of application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Client secret of the registered application.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The redirect URIs of the registered application.
        /// </summary>
        public string RedirectUris { get; set; }

        /// <summary>
        /// The client type of the registered application.
        /// </summary>
        public int ClientType { get; set; }


        /// <summary>
        /// The user name of the application's owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Creates a new <see cref="ApplicationEntity"/>.
        /// </summary>
        public ApplicationEntity()
        { }

        /// <summary>
        /// Creates a new <see cref="ApplicationEntity"/>.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> on which the new <see cref="ApplicationEntity"/> is created based.</param>
        /// <param name="normalizer">The <see cref="ILookupNormalizer"/> to normalize the client identifer.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="application"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="normalizer"/> is null.</exception>
        public static ApplicationEntity Create(Application application, ILookupNormalizer normalizer)
        {
            Guard.ArgumentNotNull(application, nameof(application));
            Guard.ArgumentNotNull(normalizer, nameof(normalizer));

            return new ApplicationEntity
            {
                ClientId = normalizer.Normalize(application.ClientId),
                ClientSecret = application.ClientSecret,
                RedirectUris = normalizer.Normalize(string.Join(Constants.SeperatorString.ToString(), application.RedirectUris.Select(it => it.ToString().ToLowerInvariant()).ToArray())),
                ClientType = (int)application.ClientType,
                Owner = normalizer.Normalize(application.Owner),
                ApplicationName = application.ApplicationName
            };
        }

        /// <summary>
        /// Concert current <see cref="ApplicationEntity"/> to an <see cref="Application"/>.
        /// </summary>
        /// <returns>The current <see cref="ApplicationEntity"/> to convert.</returns>
        public Application ToApplication()
        {
            return new Application(ClientId, ApplicationName, ClientSecret, RedirectUris.Split(Constants.SeperatorCharacter).Select(it=>new Uri(it)),  (ClientType)ClientType, Owner);
        }
    }
}
