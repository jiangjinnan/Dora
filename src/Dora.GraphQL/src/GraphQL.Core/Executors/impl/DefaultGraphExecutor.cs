using AutoMapper;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Default implementation of <see cref="IGraphExecutor"/>.
    /// </summary>
    public class DefaultGraphExecutor : IGraphExecutor
    {
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly ILogger _logger;
        private readonly Action<ILogger, DateTimeOffset, string, string, Exception> _log4ReturnRoot;
        private readonly Action<ILogger, DateTimeOffset, string, string, Exception> _log4QueryResult;
        private readonly Action<ILogger, DateTimeOffset, string, string, Exception> _log4Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphExecutor" /> class.
        /// </summary>
        /// <param name="graphTypeProvider">The graph type provider.</param>
        /// <param name="logger">The logger.</param>
        public DefaultGraphExecutor(IGraphTypeProvider graphTypeProvider, ILogger<DefaultGraphExecutor> logger)
        {
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _log4ReturnRoot = LoggerMessage.Define<DateTimeOffset, string, string>(LogLevel.Trace, 0, "[{0}]Directly return the source object. Operation: {1}. Source type: {2}");
            _log4QueryResult = LoggerMessage.Define<DateTimeOffset, string, string>(LogLevel.Trace, 0, "[{0}]Create dynamically generated class. Operation: {1}. Source type: {2}");
            _log4Error = LoggerMessage.Define<DateTimeOffset, string, string>(LogLevel.Error, 0, "[{0}]Unhandled exception. Operation: {1}. Detailed information: {2}");
        }

        /// <summary>
        /// Executes and generate final result.
        /// </summary>
        /// <param name="graphContext">The <see cref="T:Dora.GraphQL.Executors.GraphContext" /> representing the current request based execution context.</param>
        /// <returns>
        /// The <see cref="T:Dora.GraphQL.Executors.ExecutionResult" /> used as the response contents.
        /// </returns>
        public async  ValueTask<ExecutionResult> ExecuteAsync(GraphContext graphContext)
        {
            try
            {
                var selections = graphContext.SelectionSet;
                if (selections.Count == 1)
                {
                    var selection = selections.OfType<IFieldSelection>().Single();
                    var resoloverContext = new ResolverContext(graphContext, graphContext.Operation, selection, null);
                    var result = await ExecuteCoreAsync(resoloverContext);
                    return ExecutionResult.Success(result);
                }

                var dictionary = new Dictionary<string, object>();
                foreach (IFieldSelection subSelection in graphContext.SelectionSet)
                {
                    var resoloverContext = new ResolverContext(graphContext, graphContext.Operation, subSelection, null);
                    var result = await ExecuteCoreAsync(resoloverContext);
                    dictionary[subSelection.Alias ?? subSelection.Name] = result;
                }
                return ExecutionResult.Success(dictionary);
            }
            catch (GraphException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _log4Error(_logger, DateTimeOffset.Now, graphContext.OperationName, ErrorFormatter.Instance.Format(ex), null);
                }
                return ExecutionResult.Fail(ex);
            }
        }

        private async ValueTask<object> ExecuteCoreAsync(ResolverContext resolverContext)
        {
            Validate(resolverContext);
            object container = await resolverContext.Field.Resolver.ResolveAsync(resolverContext);
            if (resolverContext.Selection is IFieldSelection fieldSelection1)
            {
                if (fieldSelection1.IncludeAllFields())
                {
                    if (resolverContext.Field.GraphType.Fields.Any())
                    {
                        _log4ReturnRoot(_logger, DateTimeOffset.Now, resolverContext.GraphContext.OperationName, container.GetType().AssemblyQualifiedName, null);
                    }
                    return container;
                }

                if (fieldSelection1.TryGetQueryResultType(out var queryResultType))
                {
                    _log4QueryResult(_logger, DateTimeOffset.Now, resolverContext.GraphContext.OperationName, container.GetType().AssemblyQualifiedName, null);
                    return Mapper.Map(container, container.GetType(), queryResultType);
                }
            }

            var subFields = new List<IFieldSelection>();
            subFields.AddRange(resolverContext.Selection.SelectionSet.OfType<IFieldSelection>());                    

            if (resolverContext.Field.GraphType.IsEnumerable)
            {
                var list = new List<object>();
                foreach (var element in (IEnumerable)container)
                {
                    List<IFieldSelection> elementSubFields = null;
                    if (TryGetFragment(element, resolverContext.Selection, out var fragment1))
                    {
                        elementSubFields = new List<IFieldSelection>(subFields);
                        elementSubFields.AddRange(fragment1.SelectionSet.OfType<IFieldSelection>());
                    }
                    var allFields = elementSubFields ?? subFields;
                    if (!allFields.Any())
                    {
                        list.Add(element);
                        continue;
                    }                   
                    var dictionary = new Dictionary<string, object>();
                    foreach (var fieldSelection in allFields)
                    {
                        resolverContext.Field.GraphType.Fields.TryGetGetField(element, fieldSelection.Name, out var subField);
                        var newResolverContext = new ResolverContext(resolverContext.GraphContext, subField, fieldSelection, element);
                        if (newResolverContext.Skip())
                        {
                            continue;
                        }
                        var element1 = await ExecuteCoreAsync(newResolverContext);
                        dictionary[fieldSelection.Alias ?? fieldSelection.Name] = element1;
                    }
                    list.Add(dictionary);
                }
                return list;
            }

            if (TryGetFragment(resolverContext, resolverContext.Selection, out var fragment2))
            {
                subFields.AddRange(fragment2.SelectionSet.OfType<IFieldSelection>());
            }
            if (!subFields.Any())
            {
                return container;
            }
            var node = new Dictionary<string, object>();
            foreach (IFieldSelection subSelection in subFields)
            {
                resolverContext.Field.GraphType.Fields.TryGetGetField(container, subSelection.Name, out var subField);
                var newResolverContext = new ResolverContext(resolverContext.GraphContext,  subField, subSelection, container);
                if (newResolverContext.Skip())
                {
                    continue;
                }
                var element = await ExecuteCoreAsync(newResolverContext);
                node[subSelection.Alias??subSelection.Name] = element;
            }
            return node;
        }
        private bool TryGetFragment(object container, ISelectionNode selection, out IFragment fragment)
        {
            var fragments = selection.SelectionSet.OfType<IFragment>().ToDictionary(it=>it.GraphType, it=>it);
            if (fragments.Count == 0 || null == container)
            {
                return (fragment = null) != null;
            }
            var containerType = container.GetType();
            var graphTypes = fragments.Select(it => it.Key).ToArray();
            var graphType = SelectBest(containerType);
            if (null == graphType)
            {
                return (fragment = null) != null;
            }

            return (fragment = fragments[graphType]) != null;

            IGraphType SelectBest(Type type)
            {
                var best = graphTypes.SingleOrDefault(it => it.Type == type );
                if (null != best)
                {
                    return best;
                }

                var parent = type.BaseType;
                return parent == null
                    ? null
                    : SelectBest(parent);
            }
        }

        private void Validate(ResolverContext resolverContext)
        {
            var arguments = resolverContext.Selection.Arguments;
            var field = resolverContext.Field;
            foreach (var argument in field.Arguments.Values.Where(it => it.GraphType.IsRequired))
            {
                if (!arguments.ContainsKey(argument.Name))
                {
                    throw new GraphException($"The required argument '{argument.Name}' for the field '{field.Name}' of '{field.ContainerType.Name}' is not provided");
                }
            }
        }
    }
}