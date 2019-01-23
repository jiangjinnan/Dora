using System.Threading.Tasks;

namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Repsents GraphQL argumnet based <see cref="IArgumentBinder"/>.
    /// </summary>
    public class GraphArgumentBinder : IArgumentBinder
    {
        /// <summary>
        /// Binds the argument.
        /// </summary>
        /// <param name="context">The <see cref="T:Dora.GraphQL.ArgumentBinders.ArgumentBinderContext" />.</param>
        /// <returns>
        /// The <see cref="T:Dora.GraphQL.ArgumentBinders.ArgumentBindingResult" /> representing the argument binding result.
        /// </returns>
        public ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            if (!context.Parameter.IsGraphArgument)
            {
                return new ValueTask<ArgumentBindingResult>( ArgumentBindingResult.Failed());
            }

            var graphContext = context.ResolverContext.GraphContext;
            var argumentName = context.Parameter.ArgumentName;

            if (!graphContext.TryGetArguments(out var arguments) || !arguments.ContainsKey(argumentName))
            {
                var argumentValue = context.ResolverContext.GetArgument(argumentName);
                return new ValueTask<ArgumentBindingResult>( ArgumentBindingResult.Success(argumentValue));
            }

            return new ValueTask<ArgumentBindingResult>(ArgumentBindingResult.Failed());
        }
    }
}
