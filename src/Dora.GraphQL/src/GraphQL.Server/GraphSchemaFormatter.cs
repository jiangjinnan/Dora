using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Server;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public class GraphSchemaFormatter : IGraphSchemaFormatter
    {
        private readonly FieldNameNormalizer _nameNormalizer;
        private readonly IGraphTypeProvider _graphTypeProvider;

        public GraphSchemaFormatter(IGraphTypeProvider graphTypeProvider, IOptions<GraphServerOptions> optionsAccessor)
        {
            Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor));
            _nameNormalizer = optionsAccessor.Value.FieldNamingConvention == Options.FieldNamingConvention.PascalCase
                ? FieldNameNormalizer.PascalCase
                : FieldNameNormalizer.CamelCase;

            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
        }

        public string Format(IGraphSchema schema, GraphSchemaFormat  format)
        {
            Guard.ArgumentNotNull(schema, nameof(schema));
            return format == GraphSchemaFormat.GQL
                ? FormatAsGql(schema)
                : FormatAsInline(schema);
        }

        private  string FormatAsGql(IGraphSchema graphSchema)
        {
            var builder = new StringBuilder();
            FormatAsInline(new HashSet<string>(), builder, graphSchema);
            return builder.ToString();
        }

        private void FormatAsInline(HashSet<string> exists, StringBuilder builder, IGraphType graphType)
        {
            if (!graphType.Fields.Any())
            {
                return;
            }
            var graphTypeName = GraphValueResolver.GetGraphTypeName(graphType.Type);
            if (exists.Contains(graphTypeName))
            {
                return;
            }

            if (graphType.OtherTypes.Length > 0)
            {
                exists.Add(graphTypeName);
                builder.Append($"union {graphTypeName} = {graphType.Type.Name}");
                foreach (var type in graphType.OtherTypes)
                {
                    var typeName = GraphValueResolver.GetGraphTypeName(type);
                    builder.Append($"|{typeName}");
                }
                builder.AppendLine();

                foreach (var clrType in graphType.OtherTypes)
                {
                    var typeName = GraphValueResolver.GetGraphTypeName(clrType);
                    _graphTypeProvider.TryGetGraphType(typeName, out var type);
                    FormatAsInline(exists, builder, type);
                }
            }

            if (graphType.IsEnum)
            {
                exists.Add(graphTypeName);
                builder.AppendLine($"enum {graphType.Name}" + "{");
                foreach (var option in Enum.GetNames(graphType.Type))
                {
                    builder.AppendLine($"\t{option}");
                }
                builder.AppendLine("}");
                builder.AppendLine();
            }

            if (graphType.Type.IsInterface)
            {
                builder.AppendLine($"interface {graphTypeName}" + "{");
            }
            else
            {
                builder.AppendLine($"type {graphTypeName}" + "{");
            }
            foreach (var item in graphType.Fields)
            {
                builder.AppendLine($"\t {_nameNormalizer.NormalizeToDestination(item.Key.Name)}: {item.Value.GraphType.Name}");
            }
            builder.AppendLine("}");
            builder.AppendLine();

            foreach (var field in graphType.Fields.Values)
            {
                FormatAsInline(exists, builder, field.GraphType);
            }
        }

        private  string FormatAsInline(IGraphSchema graphSchema)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Query");
            WriteAsInline(builder, 1, graphSchema.Query);
            builder.AppendLine("Mutation");
            WriteAsInline(builder, 1, graphSchema.Mutation);
            builder.AppendLine("Subsription");
            WriteAsInline(builder, 1, graphSchema.Subsription);

            return builder.ToString();
        }

        private void WriteAsInline(StringBuilder builder, int indentLevel, IGraphType graphType)
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
                    ? $"{field.ContainerType.Name}.{_nameNormalizer.NormalizeToDestination(field.Name)}"
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
                WriteAsInline(builder, indentLevel, field.GraphType);
                indentLevel--;
            }
        }
    }
}
