using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dora.GraphQL.Executors
{
    public class QueryResultTypeGenerator : IQueryResultTypeGenerator
    {
        public Type Generate(IFieldSelection selection, GraphField field)
        {
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
