using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;


namespace Dora.OAuthServer
{
    /// <summary>
    /// A <see cref="OAuthTicket"/> specific SecureDataFormat.
    /// </summary>
    public class OAuthTicketFormat : SecureDataFormat<OAuthTicket>
    {
        /// <summary>
        /// Creates a new <see cref="OAuthTicketFormat"/>.
        /// </summary>
        /// <param name="protector">The <see cref="IDataProtector"/> to perform encryption.</param>
        public OAuthTicketFormat(IDataProtector protector) : base(OAuthTicketSerializer.Instance, protector)
        {
        }
    }
}
