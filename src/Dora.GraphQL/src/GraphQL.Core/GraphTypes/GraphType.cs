using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    public class GraphType : IGraphType
    {
        #region Fields
        private readonly Func<object, object> _valueResolver;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        /// <value>
        /// The CLR type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets the other types for union type.
        /// </summary>
        /// <value>
        /// The other types for union type.
        /// </value>
        /// <remarks>
        /// It is an empty array for non union type.
        /// </remarks>
        public Type[] OtherTypes { get; }

        /// <summary>
        /// Gets the GraphQL type name.
        /// </summary>
        /// <value>
        /// The  GraphQL type name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this is a required (non-optional) GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; }

        /// <summary>
        /// Gets a value indicating whether this is an enumerable (array) GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is enumerable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnumerable { get; }

        /// <summary>
        /// Gets a value indicating whether this is an enum GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is enum; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum { get; }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        /// <remarks>
        /// The key is field name + container CLR type. For union GraphQL types, all member types' fields are included.
        /// </remarks>
        public IDictionary<NamedType, GraphField> Fields { get; }
        #endregion

        #region Constructors
        internal GraphType(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Query: { Type = typeof(void); break; }
                case OperationType.Mutation: { Type = typeof(void); break; }
                default: { Type = typeof(void); break; }
            }
            OtherTypes = Type.EmptyTypes;
            Name = operationType.ToString();
            IsEnumerable = false;
            IsRequired = false;
            Fields = new Dictionary<NamedType, GraphField>();
            _valueResolver = _ => _;
        }

        internal GraphType(Func<object, object> valueResolver, Type type, Type[] otherTypes, string name, bool isRequired, bool isEnumerable, bool isEnum)
        {
            _valueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            OtherTypes = otherTypes ?? throw new ArgumentNullException(nameof(otherTypes));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRequired = isRequired;
            IsEnumerable = isEnumerable;
            IsEnum = isEnum;
            Fields = new Dictionary<NamedType, GraphField>();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Resolves the value based on provided raw value.
        /// </summary>
        /// <param name="rawValue">The raw value.</param>
        /// <returns>
        /// The real value.
        /// </returns>
        public object Resolve(object rawValue) => _valueResolver(rawValue);
        #endregion
    }
}
