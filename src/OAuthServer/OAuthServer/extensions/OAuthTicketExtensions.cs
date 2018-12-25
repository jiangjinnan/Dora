using Microsoft.AspNetCore.Authentication;
using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Defines some extension methods for <see cref="OAuthTicket"/>.
    /// </summary>
    public static class OAuthTicketExtensions
    {
        /// <summary>
        /// Determine whether the specified <see cref="OAuthTicket"/> has expired.
        /// </summary>
        /// <param name="tiket">The <see cref="OAuthTicket"/> to check.</param>
        /// <param name="clock">The <see cref="ISystemClock"/> used to provide standard time.</param>
        /// <returns>The valude indicating the specified <see cref="OAuthTicket"/> has expired.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="tiket"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="clock"/> is null.</exception>
        public static bool HasExpired(this OAuthTicket tiket, ISystemClock clock)
        {
            Guard.ArgumentNotNull(tiket, nameof(tiket));
            Guard.ArgumentNotNull(clock, nameof(clock));

            return tiket.ExpiresUtc < clock.UtcNow; 
        }
    }
}
