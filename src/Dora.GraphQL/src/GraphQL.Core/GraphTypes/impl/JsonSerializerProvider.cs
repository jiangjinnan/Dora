using System;
using System.Collections.Generic;
using System.Text;
using Dora.GraphQL.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Default implementation of <see cref="IJsonSerializerProvider"/>
    /// </summary>
    /// <seealso cref="Dora.GraphQL.GraphTypes.IJsonSerializerProvider" />
    public class JsonSerializerProvider : IJsonSerializerProvider
    {
        public JsonSerializer JsonSerializer { get; }
        public JsonSerializerProvider(IOptions<GraphOptions> optionsAccessor)
        {
            var naming = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.FieldNamingConvention;
            var serializer = naming == FieldNamingConvention.PascalCase
                ? new JsonSerializer()
                : new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            serializer.Converters.Add(new StringEnumConverter());
            JsonSerializer = serializer;
        }
    }
}
