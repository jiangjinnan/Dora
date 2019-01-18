using System.Threading.Tasks;

namespace Dora.GraphQL.Executors
{
    public interface IGraphExecutor
    {
        ValueTask<ExecutionResult> ExecuteAsync(GraphContext graphContext);
    }
}
