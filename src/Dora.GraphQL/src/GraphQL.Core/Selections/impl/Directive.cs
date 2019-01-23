using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// The default implementation of <see cref="IDirective"/>.
    /// </summary>
    public class Directive: IDirective
    {
        /// <summary>
        /// Gets the directive name.
        /// </summary>
        /// <value>
        /// The directive name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public IDictionary<string, NamedValueToken> Arguments  { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Directive"/> class.
        /// </summary>
        /// <param name="name">The directive name.</param>
        public Directive(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
        }
    }
}
