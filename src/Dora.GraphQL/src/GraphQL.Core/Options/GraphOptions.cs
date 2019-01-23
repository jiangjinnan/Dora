namespace Dora.GraphQL
{
    /// <summary>
    /// Reprsents GraphQL options.
    /// </summary>
    public class GraphOptions
    {
        /// <summary>
        /// Gets or sets the field name converter.
        /// </summary>
        /// <value>
        /// The field name converter.
        /// </value>
        public FieldNameConverter FieldNameConverter { get; set; } = FieldNameConverter.Default;
    }
}
