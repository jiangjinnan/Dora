using Dora.GraphQL.GraphTypes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public class GraphSchemaFormatter : IGraphSchemaFormatter
    {
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly FieldNameConverter _fieldNameConverter;
        private string _formatted;

        public GraphSchemaFormatter(IGraphTypeProvider graphTypeProvider, IOptions<GraphOptions> optionsAccessor)
        {
            Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor));
            _fieldNameConverter = optionsAccessor.Value.FieldNameConverter;
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
        }

        public string Format(IGraphSchema schema, GraphSchemaFormat  format)
        {
            Guard.ArgumentNotNull(schema, nameof(schema));
            if (null != _formatted)
            {
                return _formatted;
            }
            _formatted = format == GraphSchemaFormat.GQL
                ? FormatAsGql(schema)
                : FormatAsInline(schema);
            return _formatted;
        }

        private  string FormatAsGql(IGraphSchema graphSchema)
        {
            var types = new List<IGraphType>();
            CollectGraphTypes(new HashSet<string>(), types, graphSchema);

            var builder = new StringBuilder();
            var root = types.Single(it => it.Name == "GraphQL");
            WriteAsGQL(builder, root);
            types.Remove(root);

            var query = types.SingleOrDefault(it => it.Name == "Query");
            if (null != query)
            {
                WriteAsGQL(builder, query);
                types.Remove(query);
            }

            var mutation = types.SingleOrDefault(it => it.Name == "Mutation");
            if (null != mutation)
            {
                WriteAsGQL(builder, mutation);
                types.Remove(mutation);
            }
            var subscription = types.SingleOrDefault(it => it.Name == "Subscription");
            if (null != subscription)
            {
                WriteAsGQL(builder, subscription);
                types.Remove(subscription);
            }

            foreach (var type in types)
            {
                WriteAsGQL(builder, type);
            }

            return builder.ToString();
        }

        private void CollectGraphTypes(HashSet<string> exists, List<IGraphType> graphTypes, IGraphType graphType)
        {
            if (graphType.OtherTypes.Any())
            {
                graphTypes.Add(graphType);
                exists.Add(graphType.Name);

                CollectGraphTypes(exists, graphTypes, _graphTypeProvider.GetGraphType(graphType.Type, false, false));
                foreach (var type in graphType.OtherTypes)
                {
                    CollectGraphTypes(exists, graphTypes, _graphTypeProvider.GetGraphType(type, false, false));
                }
            }

            if (!graphType.Fields.Any())
            {
                return;
            }
            var graphTypeName = (!graphType.IsRequired && !graphType.IsEnumerable)
                ? graphType.Name 
                : GraphValueResolver.GetGraphTypeName(graphType.Type);
            if (exists.Contains(graphTypeName))
            {
                return;
            }
            graphTypes.Add(_graphTypeProvider.TryGetGraphType(graphTypeName, out var value)? value: graphType);
            exists.Add(graphTypeName);
            foreach (var field in graphType.Fields.Values)
            {
                CollectGraphTypes(exists, graphTypes, field.GraphType);
            }
        }

        private void WriteAsGQL(StringBuilder builder, IGraphType graphType)
        {
            if (graphType.OtherTypes.Length > 0)
            {
                builder.Append($"union {graphType.Name.Trim('[', ']', '!')} = {graphType.Type.Name}");
                foreach (var type in graphType.OtherTypes)
                {
                    var typeName = GraphValueResolver.GetGraphTypeName(type);
                    builder.Append($"|{typeName}");
                }
                builder.AppendLine();
                builder.AppendLine();
                return;
            }

            if (graphType.IsEnum)
            {
                builder.AppendLine($"enum {graphType.Name.Trim('[', ']', '!')}" + " {");
                foreach (var option in Enum.GetNames(graphType.Type))
                {
                    builder.AppendLine($"{new string(' ', 4)}{option}");
                }
                builder.AppendLine("}");
                builder.AppendLine();
                return;
            }

            if (graphType.Type.IsInterface)
            {
                builder.AppendLine($"interface {graphType.Name.Trim('[', ']', '!')}" + " {");
            }
            else
            {
                builder.AppendLine($"type {graphType.Name.Trim('[', ']', '!')}" + " {");
            }
            foreach (var item in graphType.Fields)
            {
                builder.AppendLine($"{new string(' ', 4)}{item.Key.Name}: {item.Value.GraphType.Name}");
            }
            builder.AppendLine("}");
            builder.AppendLine();
        }

        private  string FormatAsInline(IGraphSchema graphSchema)
        {
            var builder = new StringBuilder();
            if (graphSchema.Query.Fields.Any())
            {
                builder.AppendLine("Query");
                WriteAsInline(builder, 1, graphSchema.Query);
            }
            if (graphSchema.Mutation.Fields.Any())
            {
                builder.AppendLine("Mutation");
                WriteAsInline(builder, 1, graphSchema.Mutation);
            }
            if (graphSchema.Subscription.Fields.Any())
            {
                builder.AppendLine("Subsription");
                WriteAsInline(builder, 1, graphSchema.Subscription);
            }

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
                    ? $"[{field.ContainerType.Name}]{field.Name}"
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
                            builder.Append($"{argument.Name}:{argument.GraphType.Name})");
                        }
                        else
                        {
                            builder.Append($"{argument.Name}:{argument.GraphType.Name}, ");
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
