using System.Threading.Tasks;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Represents the GraphQL executor. 
    /// </summary>
    public interface IGraphExecutor
    {
        /// <summary>
        /// Executes and generate final result.
        /// </summary>
        /// <param name="graphContext">The <see cref="GraphContext"/> representing the current request based execution context.</param>
        /// <returns>The <see cref="ExecutionResult"/> used as the response contents.</returns>
        ValueTask<ExecutionResult> ExecuteAsync(GraphContext graphContext);
    }
}
