using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the <see cref="ResourceServerMiddleware"/> based configuration options.
    /// </summary>
    public class ResourceServerOptions
    {
        /// <summary>
        /// The <see cref="JsonSerializerSettings"/> for JSON based serialization.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; }

        /// <summary>
        /// All resource endpoints.
        /// </summary>
        public IList<ResourceEndpoint> Endpoints { get; }

        /// <summary>
        /// Creates a new <see cref="ResourceServerOptions"/>.
        /// </summary>
        public ResourceServerOptions()
        {
            Endpoints = new List<ResourceEndpoint>();
            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
