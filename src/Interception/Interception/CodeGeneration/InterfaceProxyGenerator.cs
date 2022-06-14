using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Dora.Interception.CodeGeneration
{
    [NonInterceptable]
    internal class InterfaceProxyGenerator : CodeGeneratorBase
    {
        private readonly Lazy<IVirtualMethodProxyGenerator> _virtualMethodProxyGeneratorAccessor;
        private readonly IMethodInvokerBuilder _methodInvokerBuilder;

        public InterfaceProxyGenerator(IMethodInvokerBuilder methodInvokerBuilder, IServiceProvider serviceProvider, IEnumerable<IInterceptorProvider> interceptorProviders)
            :base(interceptorProviders)
        {
            _methodInvokerBuilder = methodInvokerBuilder ?? throw new ArgumentNullException(nameof(methodInvokerBuilder));
            _virtualMethodProxyGeneratorAccessor = new Lazy<IVirtualMethodProxyGenerator>(() => serviceProvider.GetServices<ICodeGenerator>().OfType<IVirtualMethodProxyGenerator>().Single());
        }

        public override bool TryGenerate(ServiceDescriptor serviceDescriptor, CodeGenerationContext codeGenerationContext, out string[]? proxyTypeNames)
        {
            proxyTypeNames = null;
            var @interface = serviceDescriptor.ServiceType;
            if (!@interface.IsInterface)
            {
                return false;
            }
            var implementationType = serviceDescriptor.ImplementationType;
            if (implementationType == null)
            {
                return false;
            }

            Validate(implementationType);

            if (!@interface.IsPublic && !@interface.IsNestedPublic)
            {
                return false;
            }

            var methods = implementationType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(it => it.IsPublic || it.IsFamily || it.IsFamilyAndAssembly)
                .Where(it => it.DeclaringType != typeof(object))
                ;
            var interceptableMethods = methods.Where(it => _methodInvokerBuilder.CanIntercept(implementationType, it));
            if (!interceptableMethods.Any())
            {
                return false;
            }

            foreach (var method in methods)
            {
                codeGenerationContext.References.Add(method.ReturnType.Assembly);
                foreach (var parameter in method.GetParameters())
                {
                    codeGenerationContext.References.Add(parameter.ParameterType.Assembly);
                }
            }

            MethodInfo[] interfaceMethods;
            MethodInfo[] targetMethods;
            if (@interface.IsGenericTypeDefinition && implementationType.IsGenericTypeDefinition)
            {
                GetGenericInterfaceMapping(@interface, implementationType, out interfaceMethods, out targetMethods);
            }
            else
            {
                var map = implementationType.GetInterfaceMap(@interface);
                interfaceMethods = map.InterfaceMethods;
                targetMethods = map.TargetMethods;
            }          

            
            var nonInterfaceMethods = interceptableMethods.Except(targetMethods).Where(it=>it.IsVirtual);
            string? virtualMethodClassName = null;
            
            if (nonInterfaceMethods.Any())
            {
                virtualMethodClassName = _virtualMethodProxyGeneratorAccessor.Value.Generate(codeGenerationContext, implementationType, nonInterfaceMethods.ToArray());
            }

            if (!interceptableMethods.Intersect(targetMethods).Any())
            {
                proxyTypeNames = new string[] { virtualMethodClassName! };
                return true;
            }

            var interfaceProxyName = GenerateCore(codeGenerationContext, @interface, implementationType, interfaceMethods, targetMethods, virtualMethodClassName ?? implementationType.GetOutputName());
            proxyTypeNames = string.IsNullOrEmpty(virtualMethodClassName) ? new string[] { interfaceProxyName } : new string[] { interfaceProxyName, virtualMethodClassName };
            return true;
        }

        public override void RegisterProxyType(IServiceCollection services, ServiceDescriptor serviceDescriptor, Type[] proxyTypes)
        {
            var virtualMethodProxyType = proxyTypes.SingleOrDefault(it => typeof(IVirtualMethodProxy).IsAssignableFrom(it));
            var interfaceProxyType = proxyTypes.SingleOrDefault(it => typeof(IInterfaceProxy).IsAssignableFrom(it));
            var lifetime = serviceDescriptor.Lifetime;
            var @interface = serviceDescriptor.ServiceType;
            var implementationType = serviceDescriptor.ImplementationType!;

            services.Remove(serviceDescriptor);
            if (virtualMethodProxyType != null && interfaceProxyType == null)
            {
                services.Add(ServiceDescriptor.Describe(@interface, virtualMethodProxyType, lifetime));
            }
            else if (virtualMethodProxyType == null && interfaceProxyType != null)
            {
                services.TryAdd(ServiceDescriptor.Describe(implementationType, implementationType, lifetime));
                services.Add(ServiceDescriptor.Describe(@interface, interfaceProxyType, lifetime));
            }
            else if (virtualMethodProxyType != null && interfaceProxyType != null)
            {
                services.TryAdd(ServiceDescriptor.Describe(implementationType, implementationType, lifetime));
                services.Add(ServiceDescriptor.Describe(virtualMethodProxyType, virtualMethodProxyType, lifetime));
                services.Add(ServiceDescriptor.Describe(@interface, interfaceProxyType, lifetime));
            }
        }

        private string GenerateCore(CodeGenerationContext context, Type @interface, Type implementationType, MethodInfo[] interfaceMethods, MethodInfo[] targetMethods, string targetTypeName)
        {
            var proxyClassName = GetInterceptableProxyClassName(implementationType, out var constraints);
            context.References.Add(@interface.Assembly);
            context.References.Add(implementationType.Assembly);
            var interceptableMethods = targetMethods.Where(it => _methodInvokerBuilder.CanIntercept(implementationType, it)).ToArray();

            var isDisposable = typeof(IDisposable).IsAssignableFrom(implementationType);
            var isAsyncDisposable = typeof(IAsyncDisposable).IsAssignableFrom(implementationType);
            var disposableInterfaces = "";
            if (isAsyncDisposable && isAsyncDisposable)
            {
                disposableInterfaces = " , IDisposable, IAsyncDisposable";
            }
            else if (isAsyncDisposable)
            {
                disposableInterfaces = " , IDisposable";
            }
            else if (isAsyncDisposable)
            {
                disposableInterfaces = " , IAsyncDisposable";
            }
            context.WriteLines($"public class {proxyClassName} : {@interface.GetOutputName()}, IInterfaceProxy {disposableInterfaces}");
            if (constraints is not null)
            {
                using (context.Indent())
                {
                    foreach (var constraint in constraints)
                    {
                        context.WriteLines(constraint);
                    }
                }
            }
            using (context.CodeBlock())
            {
                //Fields
                GenerateFields(context, implementationType, @interface, interceptableMethods, out var methodInfoAccessorFieldNames, out var invokerAccessorFieldNames, out var invokerMethodNames);

                //InvocationContextClasses.
                var invocationContextClassNames = new Dictionary<MethodInfo, string>();
                foreach (var method in interceptableMethods)
                {
                    var className = ResolveInvocationContextClassName(method);
                    invocationContextClassNames[method] = GenerateInvocationContextClass(context, className, method);
                }

                //Constructor
                var constructorName = proxyClassName;
                if (implementationType.IsGenericTypeDefinition)
                {
                    var index = constructorName.IndexOf('<');
                    constructorName = proxyClassName[..index];
                }
                context.WriteLines($"public {constructorName}(IServiceProvider provider, IInvocationServiceScopeFactory scopeFactory)");
                using (context.CodeBlock())
                {
                    context.WriteLines($"_target = ActivatorUtilities.CreateInstance<{targetTypeName}>(provider);");
                    context.WriteLines("_scopeFactory = scopeFactory;");
                }

                //Interceptable methods
                foreach (var method in interceptableMethods)
                {
                    if (method.IsSpecialName)
                    {
                        continue;
                    }
                    string? invokerFieldName = null;
                    invokerAccessorFieldNames?.TryGetValue(method, out invokerFieldName);
                    GenerateMethod(context, implementationType, method, invocationContextClassNames[method], invokerFieldName, $"{methodInfoAccessorFieldNames[method]}.Value", @interface,true);
                }

                //Invoker methods
                foreach (var method in interceptableMethods)
                {
                    if (invokerMethodNames?.TryGetValue(method, out var invokerMethodName) ?? false)
                    {
                        var invokerAccessorFiledName = invokerAccessorFieldNames![method];
                        TryGetPropertyName(method, out var propertyName);
                        GenerateInvokerMethod(context, method, invocationContextClassNames[method], invokerMethodName, @interface, propertyName, false);
                    }
                }

                // Non-Interceptable methods
                foreach (var method in targetMethods.Except(interceptableMethods))
                {
                    if (!method.IsSpecialName)
                    {
                        GenerateMethod(context, implementationType, method, null, null, null, @interface, false);
                    }
                }

                var properties = @interface.GetProperties();
                if (properties.Any())
                {
                    var map = Enumerable.Range(0, interfaceMethods.Length).ToDictionary(it => interfaceMethods[it], it => targetMethods[it]);
                    foreach (var property in properties)
                    {
                        string? getInvocationContextClassName = null;
                        string? setInvocationContextClassName = null;
                        string? getInvokerFieldName = null;
                        string? setInvokerFieldName = null;
                        string? getMethodAccessor = null;
                        string? setMethodAccessor = null;
                        var getMethod = property.GetMethod;
                        var setMethod = property.SetMethod;
                        if (getMethod != null)
                        {
                            getMethod = map[getMethod];
                            if (methodInfoAccessorFieldNames.TryGetValue(getMethod, out var accessor))
                            {
                                getMethodAccessor = $"{accessor}.Value";
                            }
                        }
                        if (setMethod != null)
                        {
                            setMethod = map[setMethod];
                            if (methodInfoAccessorFieldNames.TryGetValue(setMethod, out var accessor))
                            {
                                setMethodAccessor = $"{accessor}.Value";
                            }
                        }
                        if (getMethod != null)
                        {
                            invocationContextClassNames.TryGetValue(getMethod, out getInvocationContextClassName);
                            invokerAccessorFieldNames?.TryGetValue(getMethod, out getInvokerFieldName);
                        }
                        if (setMethod != null)
                        {
                            invocationContextClassNames.TryGetValue(setMethod, out setInvocationContextClassName);
                            invokerAccessorFieldNames?.TryGetValue(setMethod, out setInvokerFieldName);
                        }
                        GenerateProperty(context, property, getInvocationContextClassName, setInvocationContextClassName, getMethodAccessor, setMethodAccessor, getInvokerFieldName, setInvokerFieldName, @interface);
                    }
                }

                var events = @interface.GetEvents();
                foreach (var @event in events)
                {
                    context.WriteLines($"event {@event.EventHandlerType!.GetOutputName()} {@interface.GetOutputName()}.{@event.Name}");
                    using (context.CodeBlock())
                    {
                        context.WriteLines($"add => _target.{@event.Name} += value;");
                        context.WriteLines($"remove => _target.{@event.Name} -= value;");
                    }
                }

                //Disposable
                if (isDisposable)
                {
                    context.WriteLines("public void Dispose()=> (_target as IDisposable)?.Dispose();");
                }
                if (isAsyncDisposable)
                {
                    context.WriteLines("public ValueTask DisposeAsync() => (_target as IAsyncDisposable)?.DisposeAsync()??ValueTask.CompletedTask;");
                }
            }
            var fullName = $"Dora.Interception.CodeGeneration.{proxyClassName}";
            if (implementationType.IsGenericTypeDefinition)
            {
                var index = fullName.IndexOf('<');
                fullName = $"{fullName[..index]}`{implementationType.GetGenericArguments().Length}";
            }
            return fullName;
        }

        private static void GetGenericInterfaceMapping(Type @interface, Type implementationType, out MethodInfo[] interfaceMethods, out MethodInfo[] targetMethods)
        {
            var implemented = implementationType.GetInterfaces().Single(it => it.IsGenericType && it.GetGenericTypeDefinition() == @interface);
            var mapping = implementationType.GetInterfaceMap(implemented);
            interfaceMethods = new MethodInfo[mapping.InterfaceMethods.Length];
            targetMethods = mapping.TargetMethods;
            var candidates = @interface.GetMethods();

            for (int index = 0; index < interfaceMethods.Length; index++)
            {
                var targetMethod = targetMethods[index];
                interfaceMethods[index] = candidates.First(it => Match(it, targetMethod));
            }

            static bool Match(MethodInfo method1, MethodInfo method2)
            {
                if (method1.Name != method2.Name)
                {
                    return false;
                }

                var parameters1 = method1.GetParameters();
                var parameters2 = method2.GetParameters();
                if (parameters1.Length != parameters2.Length)
                {
                    return false;
                }
                for (var index = 0; index < parameters1.Length; index++)
                {
                    var parameterType1 = parameters1[index].ParameterType;
                    var parameterType2 = parameters2[index].ParameterType;
                    if (parameterType1 == parameterType2)
                    {
                        continue;
                    }
                    if (parameterType1.IsGenericParameter && parameterType2.IsGenericParameter && parameterType1.Name == parameterType2.Name)
                    {
                        continue;
                    }
                    return false;
                }
                return true;
            }
        }
    }
}
