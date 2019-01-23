using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Default implementation of <see cref="IQueryResultTypeGenerator"/>.
    /// </summary>
    public class QueryResultTypeGenerator : IQueryResultTypeGenerator
    {
        private readonly ILogger _logger;
        private readonly Action<ILogger, DateTimeOffset, string, Exception> _log4GenerateQueryResultType;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultTypeGenerator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public QueryResultTypeGenerator(ILogger<QueryResultTypeGenerator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _log4GenerateQueryResultType = LoggerMessage.Define<DateTimeOffset, string>(LogLevel.Trace, 0, "[{0}Dynamically generate query result type. Source type: {1}]");
        }

        /// <summary>
        /// Generates the query result class generator.
        /// </summary>
        /// <param name="selection">The <see cref="T:Dora.GraphQL.Selections.IFieldSelection" /> represents the selection node.</param>
        /// <param name="field">The <see cref="T:Dora.GraphQL.GraphTypes.GraphField" /> specific to the selection node.</param>
        /// <returns>
        /// The generated query result class.
        /// </returns>
        public Type Generate(IFieldSelection selection, GraphField field)
        {
            _log4GenerateQueryResultType(_logger, DateTimeOffset.Now, field.GraphType.Type.AssemblyQualifiedName, null);
            var assemblyName = new AssemblyName($"QueryResult{GetSurffix()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");
            return Generate(selection, field, moduleBuilder);
        }

        private Type Generate(IFieldSelection selection, GraphField  graphField, ModuleBuilder moduleBuilder)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var typeName = $"{graphField.GraphType.Type.Name}{GetSurffix()}";
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);
            foreach (IFieldSelection subSelection in selection.SelectionSet)
            {
                var subField = graphField.GraphType.Fields.Values.Single(it => it.Name == subSelection.Name);
                var fieldName = $"_{subSelection.Name}";
                Type propertyType;
                if (subSelection.SelectionSet.Count > 0)
                {
                    
                    if (subField.GraphType.IsEnumerable)
                    {
                        var elementType = Generate(subSelection, subField, moduleBuilder);
                        propertyType = typeof(List<>).MakeGenericType(elementType);
                    }
                    else
                    {
                        propertyType = Generate(subSelection, subField, moduleBuilder);
                    }
                }
                else
                {
                    if (subField.GraphType.IsEnumerable)
                    {
                        propertyType = typeof(List<>).MakeGenericType(subField.GraphType.Type);
                    }
                    else
                    {
                        propertyType = subField.GraphType.Type;
                    }
                }

                var field = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);

                var get = typeBuilder.DefineMethod($"get_{subSelection.Name}", methodAttributes, propertyType, Type.EmptyTypes);
                var il = get.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ret);

                var set = typeBuilder.DefineMethod($"set_{subSelection.Name}", methodAttributes, typeof(void), new Type[] { propertyType });
                il = set.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, field);
                il.Emit(OpCodes.Ret);

                var pb = typeBuilder.DefineProperty(subSelection.Name, PropertyAttributes.None, propertyType, Type.EmptyTypes);
                pb.SetGetMethod(get);
                pb.SetSetMethod(set);
            }
            return typeBuilder.CreateTypeInfo();
        }
        static string GetSurffix() => Guid.NewGuid().ToString().Replace("-", "");
    }
}
