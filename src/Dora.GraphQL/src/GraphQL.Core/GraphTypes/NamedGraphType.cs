using System;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represents Name + IGraphType + DefaultValue group.
    /// </summary>
    public struct NamedGraphType: IEquatable<NamedGraphType>
    {
        #region Properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the graph.
        /// </summary>
        /// <value>
        /// The type of the graph.
        /// </value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedGraphType"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="graphType">Type of the graph.</param>
        /// <param name="defaultValue">The default value.</param>
        public NamedGraphType(string name, IGraphType graphType, object defaultValue = null):this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            GraphType = Guard.ArgumentNotNull(graphType, nameof(graphType));
            DefaultValue = defaultValue;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(NamedGraphType other)
        {
            return Name == other.Name && GraphType.Name == other.GraphType.Name && DefaultValue == other.DefaultValue;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => $"{Name}.{GraphType.Name}.{DefaultValue ?? ""}".GetHashCode();
        #endregion
    }
}
