using Newtonsoft.Json;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represents a JsonSerializer provider.
    /// </summary>
    public interface IJsonSerializerProvider
    {
        /// <summary>
        /// Gets the json serializer.
        /// </summary>
        /// <value>
        /// The json serializer.
        /// </value>
        JsonSerializer JsonSerializer { get; }
    }
}
