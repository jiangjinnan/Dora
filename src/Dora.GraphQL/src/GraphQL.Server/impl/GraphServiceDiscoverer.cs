using Dora.GraphQL.Descriptors;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.GraphQL.Server
{
    public class GraphServiceDiscoverer : IGraphServiceDiscoverer
    {
        private readonly Lazy<IEnumerable<GraphServiceDescriptor>> _servoicesAccessor;
        public IEnumerable<GraphServiceDescriptor> Services { get =>_servoicesAccessor.Value;  }

        public GraphServiceDiscoverer(IHostingEnvironment environment)
        {
            Guard.ArgumentNotNull(environment, nameof(environment));
            _servoicesAccessor = new Lazy<IEnumerable<GraphServiceDescriptor>>(() => {
                var assemblyName = new AssemblyName(environment.ApplicationName);
                var serviceTypes = GetGraphServiceTypes(Assembly.Load(assemblyName));
                return serviceTypes
                .Where(it => typeof(GraphServiceBase).IsAssignableFrom(it) && !it.IsAbstract)
                .Select(it => new GraphServiceDescriptor(it))
                .ToArray();
            });
        }

        private IEnumerable<Type> GetGraphServiceTypes(Assembly assembly)
        {
            var list = new List<Type>();
            AddGraphServiceTypes(list, Guard.ArgumentNotNull(assembly, nameof(assembly)));
            return list;
        }

        private void AddGraphServiceTypes(List<Type> types, Assembly assembly)
        {
            types.AddRange(assembly.GetExportedTypes().Where(it => typeof(GraphServiceBase).IsAssignableFrom(it)));
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (assemblyName.Name.StartsWith("Microsoft.") || assemblyName.Name.StartsWith("System."))
                {
                    continue;
                }
                AddGraphServiceTypes(types, Assembly.Load(assemblyName));
            }
        }
    }
}
