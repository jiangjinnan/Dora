using System;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Rerpesents Name + Type pair.
    /// </summary>
    public struct NamedType:IEquatable<NamedType>
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
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedType"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public NamedType(string name, Type type) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
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
        public bool Equals(NamedType other) => Name == other.Name && Type == other.Type;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => $"{Name}.{Type.AssemblyQualifiedName}".GetHashCode();
        #endregion
    }
}
