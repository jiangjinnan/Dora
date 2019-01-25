using GraphQL.Language.AST;
using GraphQL.Types;
using System;

namespace Dora.GraphQL.Server
{
    internal class GuidGraphType: ScalarGraphType
    {
        public GuidGraphType() => Name = "Guid";
        public override object Serialize(object value) => ParseValue(value);
        public override object ParseValue(object value)
        {
            if (null == value)
            {
                return null;
            }

            return Guid.TryParse(value.ToString(), out var guid)
                ? (object)guid
                : null;
        }
        public override object ParseLiteral(IValue value) => ParseValue(value.Value);
    }
}
