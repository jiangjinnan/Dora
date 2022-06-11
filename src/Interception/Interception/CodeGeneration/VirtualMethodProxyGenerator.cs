using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dora.Interception.CodeGeneration
{
    internal class VirtualMethodProxyGenerator : CodeGeneratorBase, IVirtualMethodProxyGenerator, ICodeGenerator
    {
        private readonly IMethodInvokerBuilder _methodInvokerBuilder;

        public VirtualMethodProxyGenerator(IMethodInvokerBuilder methodInvokerBuilder, IEnumerable<IInterceptorProvider> interceptorProviders)
            : base(interceptorProviders)
        {
            _methodInvokerBuilder = methodInvokerBuilder ?? throw new ArgumentNullException(nameof(methodInvokerBuilder));
        }

        public string Generate(CodeGenerationContext context, Type baseType, MethodInfo[] interceptableMethods)
        {
            context.References.Add(baseType.Assembly);
            var proxyClassName = GetInterceptableProxyClassName(baseType, out var constraints);
            context.WriteLines($"public sealed class {proxyClassName} : {baseType.GetOutputName()}, IVirtualMethodProxy");

            #region  Generic argument constraints.
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
            #endregion

            using (context.CodeBlock())
            {      
                #region Fields
                GenerateFields(context, baseType, null, interceptableMethods, out var methodInfoAccessorFieldNames, out var invokerAccessorFieldNames, out var invokerMethodNames);
                context.WriteLines();
                #endregion

                #region InvocationContextClasses.
                var invocationContextClassNames = new Dictionary<MethodInfo, string>();
                foreach (var method in interceptableMethods)
                {
                    var className = ResolveInvocationContextClassName(method);
                    invocationContextClassNames[method] = GenerateInvocationContextClass(context, className, method);
                    context.WriteLines();
                }
                #endregion

                #region Properties
                var properties = baseType.GetProperties();
                if (properties.Any())
                {
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
                            if (methodInfoAccessorFieldNames.TryGetValue(getMethod, out var accessor))
                            {
                                getMethodAccessor = $"{accessor}.Value";
                            }
                        }
                        if (setMethod != null)
                        {
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

                        if (!string.IsNullOrWhiteSpace(setInvocationContextClassName))
                        {
                            GenerateProperty(context, property, getInvocationContextClassName, setInvocationContextClassName, getMethodAccessor, setMethodAccessor, getInvokerFieldName, setInvokerFieldName, null);
                        }
                    }
                }
                #endregion

                #region Constructors
                var constructorName = proxyClassName;
                if (baseType.IsGenericTypeDefinition)
                {
                    var index = constructorName.IndexOf('<');
                    constructorName = proxyClassName[..index];
                }
                foreach (var constructor in baseType.GetConstructors())
                {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 0)
                    {
                        context.WriteLines($"{constructor.GetModifier()} {constructorName} (IInvocationServiceScopeFactory scopeFactory) :base()");
                    }
                    else
                    {
                        var parameterTypeAndNames = string.Join(", ", parameters.Select(it => $"{it.ParameterType.GetOutputName()} {it.Name}"));
                        var parameterNames = string.Join(", ", parameters.Select(it => it.Name));
                        context.WriteLines($"{constructor.GetModifier()} {constructorName} (IInvocationServiceScopeFactory scopeFactory, {parameterTypeAndNames}) :base({parameterNames})");
                    }
                    using (context.CodeBlock())
                    {
                        context.WriteLines("_scopeFactory = scopeFactory;");
                        if (invokerAccessorFieldNames != null)
                        {
                            foreach (var kv in invokerAccessorFieldNames)
                            {
                                var method = kv.Key;
                                var fieldName = kv.Value;
                                var methodName = invokerMethodNames![method];
                                context.WriteLines($"{fieldName} = new Lazy<InvokeDelegate>(() => MethodInvokerBuilder.Instance.Build(typeof({baseType.GetOutputName()}), ProxyHelper.GetMethodInfo<{baseType.GetOutputName()}>({method.MetadataToken}), {methodName}));");
                            }
                        }
                    }
                    context.WriteLines();
                }
                #endregion

                #region Interceptable methods
                foreach (var method in interceptableMethods)
                {
                    if (method.IsSpecialName)
                    {
                        continue;
                    }
                    string? invokerFieldName = null;
                    invokerAccessorFieldNames?.TryGetValue(method, out invokerFieldName);
                    GenerateMethod(context, baseType, method, invocationContextClassNames[method], invokerFieldName, $"{methodInfoAccessorFieldNames[method]}.Value", null, true);
                    context.WriteLines();
                }
                #endregion

                #region Invoker methods
                foreach (var method in interceptableMethods)
                {
                    if (invokerMethodNames?.TryGetValue(method, out var invokerMethodName) ?? false)
                    {
                        var invokerAccessorFiledName = invokerAccessorFieldNames![method];
                        TryGetPropertyName(method, out var propertyName);
                        GenerateInvokerMethod(context, method, invocationContextClassNames[method], invokerMethodName, null, propertyName, false);
                    }
                }
                #endregion
            }

            var fullName = $"Dora.Interception.CodeGeneration.{proxyClassName}";
            if (baseType.IsGenericTypeDefinition)
            {
                var index = fullName.IndexOf('<');
                fullName = $"{fullName[..index]}`{baseType.GetGenericArguments().Length}";
            }
            return fullName;
        }

        public override void RegisterProxyType(IServiceCollection services, ServiceDescriptor serviceDescriptor, Type[] proxyTypes)
        {
            services.Remove(serviceDescriptor);
            services.Add(ServiceDescriptor.Describe(serviceDescriptor.ServiceType, proxyTypes[0], serviceDescriptor.Lifetime));
        }

        public override bool TryGenerate(ServiceDescriptor serviceDescriptor, CodeGenerationContext codeGenerationContext, out string[]? proxyTypeNames)
        {
            proxyTypeNames = null;
            var serviceType = serviceDescriptor.ServiceType;
            if (serviceType.IsInterface)
            {
                return false;
            }
            var implementationType = serviceDescriptor.ImplementationType;
            if (implementationType == null)
            {
                return false;
            }

            Validate(implementationType);

            var interceptableMethods = implementationType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(it => _methodInvokerBuilder.CanIntercept(implementationType, it) && it.IsVirtual)
                .ToArray();
            if (!interceptableMethods.Any())
            {
                return false;
            }

            foreach (var method in interceptableMethods)
            {                
                codeGenerationContext.References.Add(method.ReturnType.Assembly);
                foreach (var parameter in method.GetParameters())
                {
                    codeGenerationContext.References.Add(parameter.ParameterType.Assembly);
                }
            }

            var proxyTypeName = Generate(codeGenerationContext, implementationType, interceptableMethods);
            proxyTypeNames = new string[] { proxyTypeName };
            return true;
        }
    }
}
