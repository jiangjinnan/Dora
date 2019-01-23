using Dora.GraphQL.GraphTypes;
using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Defines <see cref="IGraphType"/> specific extension methods.
    /// </summary>
    public static class GraphTypeExtensions
    {
        /// <summary>
        /// Adds a new the field in the given <see cref="IGraphType"/>.
        /// </summary>
        /// <param name="graphType">The <see cref="IGraphType"/> in which the new field is added.</param>
        /// <param name="containerType">The CLR type in which the added field is defined.</param>
        /// <param name="graphField">The <see cref="GraphField"/> to add.</param>
        /// <returns>The given <see cref="IGraphType"/>.</returns>
        public static IGraphType AddField(this IGraphType graphType, Type containerType, GraphField graphField)
        {
            Guard.ArgumentNotNull(graphType, nameof(graphType));
            Guard.ArgumentNotNull(containerType, nameof(containerType));
            Guard.ArgumentNotNull(graphField, nameof(graphField));
            graphType.Fields.Add(new NamedType(graphField.Name, containerType), graphField);
            return graphType;
        }

        /// <summary>
        /// Determines whether [is union type].
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        /// <returns>
        ///   <c>true</c> if [is union type] [the specified graph type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUnionType(this IGraphType graphType) => Guard.ArgumentNotNull(graphType, nameof(graphType)).OtherTypes.Length > 0;

    }
}
