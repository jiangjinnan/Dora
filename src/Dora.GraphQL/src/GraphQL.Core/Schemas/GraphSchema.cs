using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public sealed class GraphSchema : IGraphSchema
    {
        public GraphSchema(IGraphType query, IGraphType mutation, IGraphType subsription)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
            Subsription = subsription ?? throw new ArgumentNullException(nameof(subsription));
        }

        public IGraphType Query { get; }
        public IGraphType Mutation { get; }
        public IGraphType Subsription { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Query");
            Write(builder, 1, Query);
            builder.AppendLine("Mutation");
            Write(builder, 1, Mutation);
            builder.AppendLine("Subsription");
            Write(builder, 1, Subsription);

            return builder.ToString();
        }

        private void Write(StringBuilder builder, int indentLevel, IGraphType graphType)
        {
            void Indent(int level)
            {
                builder.Append(new string(' ', level * 4));
            }

            var isUnionType = graphType.OtherTypes.Length > 0;

            foreach (var field in graphType.Fields.Values)
            {
                Indent(indentLevel);
                var fieldName = isUnionType
                    ? $"{field.ContainerType.Name}.{field.Name}"
                    : field.Name;

                builder.Append($"{fieldName}: {field.GraphType.Name}");
                if (field.Arguments.Any())
                {
                    builder.Append("(");
                    var index = 0;  
                    foreach (var argument in field.Arguments.Values)
                    {
                        index++;
                        if (index == field.Arguments.Count)
                        {
                            builder.Append($"${argument.Name}:{argument.GraphType.Name})");
                        }
                        else
                        {
                            builder.Append($"${argument.Name}:{argument.GraphType.Name}, ");
                        }
                    }
                }
                builder.AppendLine();

                indentLevel++;
                Write(builder, indentLevel, field.GraphType);
                indentLevel--;
            }
        }
    }
}
