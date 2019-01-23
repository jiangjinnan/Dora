using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Represents the query result class generator.
    /// </summary>
    public interface IQueryResultTypeGenerator
    {
        /// <summary>
        /// Generates the query result class generator.
        /// </summary>
        /// <param name="selection">The <see cref="IFieldSelection"/> represents the selection node.</param>
        /// <param name="field">The <see cref="GraphField"/> specific to the selection node.</param>
        /// <returns>The generated query result class.</returns>
        Type Generate(IFieldSelection selection, GraphField field);
    }
}
