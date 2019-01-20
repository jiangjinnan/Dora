using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public class SchemaFactory : ISchemaFactory
    {
        private readonly IAttributeAccessor _attributeAccessor;
        private readonly IGraphTypeProvider _graphTypeProvider;
        public SchemaFactory(
            IAttributeAccessor attributeAccessor, 
            IGraphTypeProvider graphTypeProvider)
        {
            _attributeAccessor = attributeAccessor ?? throw new ArgumentNullException(nameof(attributeAccessor));
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
        }

        public IGraphSchema Create(Assembly assembly)
        {
            var methods = GetGraphServiceTypes(assembly)
                .SelectMany(it => GetGraphOperationMethods(it))
                .GroupBy(it => it.Value, it => it.Key)
                .ToDictionary(it => it.Key, it => it);

            IGraphType CreateGraphType(OperationType operationType, IEnumerable<MethodInfo> methodInfos)
            {
                var graphType = GraphType.CreateGraphType(operationType);
                if (methodInfos == null)
                {
                    return graphType;
                }
                foreach (var method in methodInfos)
                {
                    var operationAttribute = _attributeAccessor.GetAttribute<GraphOperationAttribute>(method, false);
                    var name = operationAttribute.Name ?? method.Name;
                    var resolver = new OperationResolver(method);
                    var type = _graphTypeProvider.GetGraphType(method.ReturnType,null,null);

                    var field = new GraphField(name, type, typeof(void), resolver);
                    foreach (var parameter in method.GetParameters())
                    {
                        var argumentAttribute = _attributeAccessor.GetAttribute<ArgumentAttribute>(parameter);
                        if (null == argumentAttribute)
                        {
                            continue;
                        }
                        var parameterGraphType =  _graphTypeProvider.GetGraphType(parameter.ParameterType, argumentAttribute.IsRequired, argumentAttribute.GetIsEnumerable());
                        field.AddArgument(new NamedGraphType(argumentAttribute.Name ?? parameter.Name, parameterGraphType));
                    }

                    graphType.AddField(typeof(void),field);
                }
                return graphType;
            }
            var query = CreateGraphType(OperationType.Query, methods.TryGetValue(OperationType.Query, out var queryMethods) ? queryMethods : null);
            var mutation = CreateGraphType(OperationType.Query, methods.TryGetValue(OperationType.Mutation, out var mutationMethods) ? mutationMethods : null);
            var subscription = CreateGraphType(OperationType.Query, methods.TryGetValue(OperationType.Mutation, out var subscirptionMethods) ? subscirptionMethods : null);
            return new GraphSchema(query, mutation, subscription);
        }

        protected virtual bool IsGraphService(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            return typeof(GraphServiceBase).IsAssignableFrom(serviceType);
        }

        protected virtual IDictionary<MethodInfo, OperationType> GetGraphOperationMethods(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            return (from method in serviceType.GetMethods()
                    let operationType = _attributeAccessor.GetAttribute<GraphOperationAttribute>(method, false)?.OperationType
                    where operationType != null
                    select (method, operationType)).ToDictionary(it => it.method, it => it.operationType.Value);
        }

        private  IEnumerable<Type> GetGraphServiceTypes(Assembly assembly)
        {
            var list = new List<Type>();
            AddGraphServiceTypes(list, Guard.ArgumentNotNull(assembly, nameof(assembly)));
            return list;
        }

        private void AddGraphServiceTypes(List<Type> types, Assembly assembly)
        {
            types.AddRange(assembly.GetExportedTypes().Where(it=>typeof(GraphServiceBase).IsAssignableFrom(it)));
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (assemblyName.Name.StartsWith("Microsoft") || assemblyName.Name.StartsWith("System"))
                {
                    continue;
                }
                AddGraphServiceTypes(types, Assembly.Load(assemblyName));
            }
        }
    }
}