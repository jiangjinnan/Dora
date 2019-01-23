using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Attribute used to define GraphQL argument.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class ArgumentAttribute : Attribute
    {
        private bool _isEnumerable;

        /// <summary>
        /// Gets or sets the argument name.
        /// </summary>
        /// <value>
        /// The argument name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the argument type.
        /// </summary>
        /// <value>
        /// The argument type.
        /// </value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this argument is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this argument is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether argument is enumerable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if argument is enumerable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnumerable
        {
            get { return _isEnumerable; }
            set {
                IsEnumerableSpecified = true;
                _isEnumerable = value;
            }
        }

        internal bool IsEnumerableSpecified { get; private set; }
        internal bool? GetIsEnumerable() => IsEnumerableSpecified ? (bool?)_isEnumerable :null;
    }
}