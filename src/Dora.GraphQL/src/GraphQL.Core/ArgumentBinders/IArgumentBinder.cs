using System.Threading.Tasks;

namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Represents the argument binder.
    /// </summary>
    public interface IArgumentBinder
    {
        /// <summary>
        /// Binds the argument.
        /// </summary>
        /// <param name="context">The <see cref="ArgumentBinderContext"/>.</param>
        /// <returns>The <see cref="ArgumentBindingResult"/> representing the argument binding result.</returns>
        ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context);
    }
}
