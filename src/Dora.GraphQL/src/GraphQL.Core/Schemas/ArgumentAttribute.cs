using System;

namespace Dora.GraphQL.Schemas
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class ArgumentAttribute : Attribute
    {
        private bool _isEnumerable;
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsRequired { get; set; } 
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