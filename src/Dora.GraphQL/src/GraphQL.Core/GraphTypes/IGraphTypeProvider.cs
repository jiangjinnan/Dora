using System;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represents a provider to create and provide <see cref="IGraphType"/>.
    /// </summary>
    public interface IGraphTypeProvider
    {
        /// <summary>
        /// Create a new <see cref="IGraphType"/> or get an existing <see cref="IGraphType"/> based on the given CLR type.
        /// </summary>
        /// <param name="type">The <see cref="IGraphType"/> specific CLR type.</param>
        /// <param name="isRequired">Indicate whether to create a required based <see cref="IGraphType"/>.</param>
        /// <param name="isEnumerable">Indicate whether to create an array based <see cref="IGraphType"/>.</param>
        /// <param name="otherTypes">The other CLR types for union GraphQL type.</param>
        /// <returns>The <see cref="IGraphType"/> to be created to provided.</returns>
        IGraphType GetGraphType(Type type, bool? isRequired, bool? isEnumerable, params Type[] otherTypes);

        /// <summary>
        /// Tries to get the created <see cref="IGraphType"/> based on specified GraphQL type name.
        /// </summary>
        /// <param name="name">The GraphQL type name.</param>
        /// <param name="graphType">The <see cref="IGraphType"/>.</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether to successfully get the areadly created <see cref="IGraphType"/>.</returns>
        bool TryGetGraphType(string name, out IGraphType graphType);
    }
}
