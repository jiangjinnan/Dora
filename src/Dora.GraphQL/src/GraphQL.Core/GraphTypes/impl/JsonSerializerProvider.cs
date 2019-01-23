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
        /// <summary>
        /// Gets the json serializer.
        /// </summary>
        /// <value>
        /// The json serializer.
        /// </value>
        public JsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerProvider"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options accessor.</param>
        public JsonSerializerProvider(IOptions<GraphOptions> optionsAccessor)
        {
            var fielNameConverter = Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor)).Value.FieldNameConverter;
            var serializer = fielNameConverter == FieldNameConverter.Default
                ? new JsonSerializer()
                : new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            serializer.Converters.Add(new StringEnumConverter());
            JsonSerializer = serializer;
        }
    }
}
