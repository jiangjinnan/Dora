using Dora.GraphQL.Selections;

namespace Dora.GraphQL
{
    /// <summary>
    /// Defines <see cref="IDirective"/> specific extension methods.
    /// </summary>
    public static  class DirectiveExtensions
    {
        /// <summary>
        /// Adds the argument to specified <see cref="IDirective"/>.
        /// </summary>
        /// <param name="directive">The <see cref="IDirective"/> to which the argumetn is added.</param>
        /// <param name="argument">The argument.</param>
        /// <returns>The <see cref="IDirective"/>.</returns>
        public static IDirective AddArgument(this IDirective directive, NamedValueToken argument)
        {
            Guard.ArgumentNotNull(directive, nameof(directive));
            directive.Arguments.Add(argument.Name, argument);
            return directive;
        }
    }
}
