using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents an OAuth tiket like Authorization Code, Refresh Token and Acces Token.
    /// </summary>
    public class OAuthTicket
    {
        #region Properties
        /// <summary>
        /// The user name to whom the ticket is issued.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The fingerprint of the ticket.
        /// </summary>
        public string Fingerprint { get; }

        /// <summary>
        /// The UTC time when the tiket expires.
        /// </summary>
        public DateTimeOffset ExpiresUtc { get; }

        /// <summary>
        /// The OAuth scope.
        /// </summary>
        public string[] Scopes { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new <see cref="OAuthTicket"/>.
        /// </summary>
        /// <param name="userName">The user name to whom the ticket is issued.</param>
        /// <param name="fingerprint">The fingerprint of the ticket.</param>
        /// <param name="expiresAtTime">The UTC time when the tiket expires.</param>
        /// <param name="scopes">The OAuth scope.</param>
        public OAuthTicket(string userName, string fingerprint, DateTimeOffset expiresAtTime, IEnumerable<string> scopes)
        {
            UserName = Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Fingerprint = Guard.ArgumentNotNullOrWhiteSpace(fingerprint, nameof(fingerprint));
            ExpiresUtc = Guard.ArgumentMustBeUtc(expiresAtTime, nameof(expiresAtTime));
            Scopes = Guard.ElementNotNullOrWhiteSpace(Guard.ArgumentNotNullOrEmpty(scopes, nameof(scopes)), nameof(scopes)).ToArray();
        }

        /// <summary>
        /// Create a new <see cref="OAuthTicket"/>.
        /// </summary>
        /// <param name="userName">The user name to whom the ticket is issued.</param>
        /// <param name="fingerprint">The fingerprint of the ticket.</param>
        /// <param name="expiresAtTime">The UTC time when the tiket expires.</param>
        /// <param name="scope">The first OAuth scope.</param>
        /// <param name="otherScopes">The other OAuth scopes.</param>
        public OAuthTicket(string userName, string fingerprint, DateTimeOffset expiresAtTime, string scope, params string[] otherScopes)
        {
            UserName = Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Fingerprint = Guard.ArgumentNotNullOrWhiteSpace(fingerprint, nameof(fingerprint));
            ExpiresUtc = Guard.ArgumentMustBeUtc(expiresAtTime, nameof(expiresAtTime));

            List<string> list = new List<string>(Guard.ElementNotNullOrWhiteSpace(otherScopes, nameof(otherScopes)));
            list.Add(Guard.ArgumentNotNullOrWhiteSpace(scope, nameof(scope)));
            Scopes = list.ToArray();
        }
        #endregion
    }
}
