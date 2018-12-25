using Microsoft.AspNetCore.Authentication;
using System;
using System.IO;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The <see cref="OAuthTicket"/> specific data serializer.
    /// </summary>
    public class OAuthTicketSerializer : IDataSerializer<OAuthTicket>
    {
        #region fields
        private static readonly OAuthTicketSerializer _instance = new OAuthTicketSerializer();
        #endregion

        #region Constructors
        private OAuthTicketSerializer() { }
        #endregion

        #region Properties
        /// <summary>
        /// The default and singleton <see cref="OAuthTicketSerializer"/>.
        /// </summary>
        public static OAuthTicketSerializer Instance
        {
            get { return _instance; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Deserialize and generate a new <see cref="OAuthTicket"/>.
        /// </summary>
        /// <param name="data">The byte array to deserialize.</param>
        /// <returns>The <see cref="OAuthTicket"/> to generate.</returns>
        public OAuthTicket Deserialize(byte[] data)
        {
            Guard.ArgumentNotNull(data, nameof(data));

            using(var stream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    string userName = reader.ReadString();
                    long ticks = reader.ReadInt64();
                    string fingerprint = reader.ReadString();
                    int count = reader.ReadInt32();
                    string[] scopes = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        scopes[i] = reader.ReadString();
                    }
                    return new OAuthTicket(userName, fingerprint, new DateTimeOffset(ticks, TimeSpan.FromTicks(0)), scopes);
                }
            }
        }

        /// <summary>
        /// Serialize the specified <see cref="OAuthTicket"/>.
        /// </summary>
        /// <param name="model">The <see cref="OAuthTicket"/> to serialize.</param>
        /// <returns>The byte array.</returns>
        public byte[] Serialize(OAuthTicket model)
        {
            Guard.ArgumentNotNull(model, nameof(model));
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(model.UserName);
                    writer.Write(model.ExpiresUtc.Ticks);
                    writer.Write(model.Fingerprint);
                    writer.Write(model.Scopes.Length);
                    foreach (var scope in model.Scopes)
                    {
                        writer.Write(scope);
                    }
                }
                 byte[] result = stream.ToArray();
                return result;
            }
        }
        #endregion
    }
}
