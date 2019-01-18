using Dora.OAuthServer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represent a registered client, which is an application making protected resource requests on behalf of the resource owner and with its authorization.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The client identifier of the application.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// A display name of the application.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// The client secret of the application.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// The uri of redirect endpoint which is used to receive authorization code or access token.
        /// </summary>
        public IList<Uri> RedirectUris { get; }

        /// <summary>
        /// The client type of the application.
        /// </summary>
        public ClientType ClientType { get; }

        /// <summary>
        /// The user name of the application's owner.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Create a new <see cref="Application"/>.
        /// </summary>
        /// <param name="clientId">The client identifier of the application.</param>
        /// <param name="applicationName">A display name of the application.</param>
        /// <param name="clientSecret">The client secret of the application.</param>
        /// <param name="redirectUris">The uri of redirect endpoint which is used to receive authorization code or access token.</param>
        /// <param name="clientType">The client type of the application.</param>
        /// <param name="owner">The user name of the application's owner.</param>
        public Application(string clientId, string applicationName, string clientSecret, IEnumerable<Uri> redirectUris, ClientType clientType, string owner)
        {
            ClientId = Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            ClientSecret = Guard.ArgumentNotNullOrWhiteSpace(clientSecret, nameof(clientSecret));
            RedirectUris = Guard.ArgumentNotNullOrEmpty(redirectUris, nameof(redirectUris)).ToList();
            Owner = Guard.ArgumentNotNullOrWhiteSpace(owner, nameof(owner));
            ApplicationName = Guard.ArgumentNotNullOrWhiteSpace(applicationName, nameof(applicationName));
            ClientType = clientType;

            if (redirectUris.Any(it => !it.IsAbsoluteUri))
            {
                throw new ArgumentException(Resources.ExceptionNotAbsoluteUri, nameof(redirectUris));
            }
        }
    }
}