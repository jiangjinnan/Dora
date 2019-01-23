using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// The default implemention of <see cref="IFieldSelection"/>.
    /// </summary>
    public class FieldSelection : IFieldSelection
    {
        /// <summary>
        /// Gets the field name.
        /// </summary>
        /// <value>
        /// The field name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        public IDictionary<string, NamedValueToken> Arguments { get; }

        /// <summary>
        /// Gets the directives.
        /// </summary>
        /// <value>
        /// The directives.
        /// </value>
        public ICollection<IDirective> Directives { get; }

        /// <summary>
        /// Gets the sub selection set.
        /// </summary>
        /// <value>
        /// The sub selection set.
        /// </value>
        public ICollection<ISelectionNode> SelectionSet { get; }

        /// <summary>
        /// Gets the custom property dictionary.
        /// </summary>
        /// <value>
        /// The custom property dictionary.
        /// </value>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelection"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FieldSelection(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
            Directives = new Collection<IDirective>();
            SelectionSet = new Collection<ISelectionNode>();
            Properties = new Dictionary<string, object>();
        }
    }
}
