using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Dora.Interception.CodeGeneration
{
    /// <summary>
    /// The base class of concrete code generator classes.
    /// </summary>
    public abstract class CodeGeneratorBase : ICodeGenerator
    {
        #region Fields
        private readonly IInterceptorProvider[] _interceptorProviders;
        private static readonly ThreadLocal<StringBuilder> _stringBuilderAccessor = new(()=> new StringBuilder());
        private static readonly Action<MethodInfo> _methodValidator = CodeGenerationUtilities.EnsureMethodInterceptable;
        private static readonly Action<PropertyInfo> _propertyValidator = CodeGenerationUtilities.EnsurePropertyInterceptable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGeneratorBase"/> class.
        /// </summary>
        /// <param name="interceptorProviders">The interceptor providers.</param>
        protected CodeGeneratorBase(IEnumerable<IInterceptorProvider> interceptorProviders)
            => _interceptorProviders = (interceptorProviders ?? throw new ArgumentNullException(nameof(interceptorProviders))).ToArray();
        #endregion

        #region Public methods
        /// <summary>
        /// Registers the dynamically generated proxy types.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> holding all service registrations.</param>
        /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor" /> representing original service registration.</param>
        /// <param name="proxyTypes">The dynamically generated interceptable proxy types.</param>
        public abstract void RegisterProxyType(IServiceCollection services, ServiceDescriptor serviceDescriptor, Type[] proxyTypes);

        /// <summary>
        /// Tries to generate source code of interceptable proxy class.
        /// </summary>
        /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor" /> representing the raw service registration.</param>
        /// <param name="codeGenerationContext">The code generation context.</param>
        /// <param name="proxyTypeNames">The names of generated classes.</param>
        /// <returns>
        /// A <see cref="Boolean" /> value indicating whether generate class.
        /// </returns>
        public abstract bool TryGenerate(ServiceDescriptor serviceDescriptor, CodeGenerationContext codeGenerationContext, out string[]? proxyTypeNames);
        #endregion

        #region Protected methods
        /// <summary>
        /// Validates the specified type and make sure interceptors are all appled to valid methods and properties.
        /// </summary>
        /// <param name="type">The type to validate.</param>
        protected void Validate(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }            
            foreach (var provider in _interceptorProviders)
            {
                provider.Validate(type, _methodValidator, _propertyValidator);
            }
        }
        #endregion

        #region Internal methods
        internal static void GenerateMethod(CodeGenerationContext context, Type targetType, MethodInfo method, string? contextClassName, string? invokerFieldName, string? methodAccessor, Type? @interface, bool isInterceptable)
        {
            var returnType = method.ReturnType;
            var parameters = method.GetParameters();

            var parameterDeclare = from parameter in parameters
                let name = parameter.Name
                let type = parameter.ParameterType
                let isOut = parameter.IsOut
                let isIn = parameter.IsIn
                let isRef = type.Name.EndsWith("&")
                select isOut ? $"out {type.GetOutputName()} {name}" : isIn ? $"in {type.GetOutputName()} {name}" : isRef ? $"ref {type.GetOutputName()} {name}" : $"{type.GetOutputName()} {name}";

            var callTargetArguments = from parameter in parameters
                let name = parameter.Name
                let isIn = parameter.IsIn
                let isOut = parameter.IsOut
                let isRef = parameter.ParameterType.Name.EndsWith("&")
                select isIn ? $"in {name}" : isOut ? $"out {name}" : isRef ? $"ref {name}" : name;

            var @async = returnType.IsAwaitable() && isInterceptable ? " async " : " ";
            var modifier = @interface != null ? "public" : method.GetModifier();
            var @override = @interface == null ? " override " : "";
            if (method.IsGenericMethod)
            {
                var genericArguments = ResolveGenericArguments(method.GetGenericArguments(), out var constrains);                
                context.WriteLines($"{modifier}{@async}{@override}{returnType.GetOutputName()} {GetNormallizedMemberName(method)}<{genericArguments}>({string.Join(", ", parameterDeclare)})");
                if (@interface != null && (constrains?.Any() ?? false))
                {
                    using (context.Indent())
                    {
                        foreach (var constrain in constrains)
                        {
                            context.WriteLines(constrain);
                        }
                    }
                }

                if (!isInterceptable)
                {
                    context.WriteLines($"=>_target.{GetNormallizedMemberName(method)}<{genericArguments}>({string.Join(", ", callTargetArguments)});");
                    return;
                }
            }
            else
            {
                context.WriteLines($"{modifier}{@async}{@override}{returnType.GetOutputName()} {GetNormallizedMemberName(method)}({string.Join(", ", parameterDeclare)})");
                if (!isInterceptable)
                {
                    context.WriteLines($"=>_target.{GetNormallizedMemberName(method)}({string.Join(", ", string.Join(", ", callTargetArguments))});");
                    return;
                }
            }           

            using (context.CodeBlock())
            {
                context.WriteLines("using var scope = _scopeFactory.CreateInvocationScope();");
                var arguments = string.Join(", ", parameters.Where(it => !it.IsOut).Select(it => it.Name));
                var target = @interface == null ? "this" : "_target";

                context.WriteLines($"var method = {methodAccessor};");
                if (invokerFieldName == null)
                {
                    var genericParameters = string.Join(", ", method.GetGenericArguments().Select(it => it.GetOutputName()));
                    context.WriteLines($"method = GenericMethodMaker.MakeGenericMethod<{genericParameters}>(method);");
                }

                if (string.IsNullOrWhiteSpace(arguments))
                {
                    context.WriteLines($"var context = new {contextClassName}({target}, method, scope.ServiceProvider);");
                }
                else
                {
                    context.WriteLines($"var context = new {contextClassName}({target}, {arguments}, method, scope.ServiceProvider);");
                }

                if (invokerFieldName != null)
                {
                    context.WriteLines($"var valueTask = {invokerFieldName}.Value.Invoke(context);");
                }
                else
                {
                    context.WriteLines($"var invoker = MethodInvokerBuilder.Instance.Build(typeof({targetType.GetOutputName()}),method, InvokeAsync);");
                    context.WriteLines($"var valueTask = invoker(context);");
                }

                foreach (var parameter in parameters.Where(it => it.ParameterType.Name.EndsWith("&") && !it.IsIn))
                {
                    context.WriteLines($"{parameter.Name} = context._{parameter.Name};");
                }

                var returnKind = returnType.AsReturnKind();
                _ = returnKind switch
                {
                    ReturnKind.ValueTask => context.WriteLines("return valueTask;"),
                    ReturnKind.Task => context.WriteLines("return valueTask.AsTask();"),
                    ReturnKind.Result=> context
                        .WriteLines($"if (!valueTask.IsCompleted) valueTask.GetAwaiter().GetResult();")
                        .WriteLines("return context._returnValue;"),
                    ReturnKind.Void=> context.WriteLines($"valueTask.GetAwaiter().GetResult();"),
                    _ => context
                        .WriteLines("if (!valueTask.IsCompleted) await valueTask;")
                        .WriteLines("return context._returnValue.Result; ")
                };               

                //InvokeAsync method
                if (string.IsNullOrEmpty(invokerFieldName))
                {
                    GenerateInvokerMethod(context, method, contextClassName!, "InvokeAsync", @interface, null, true);
                }
            }
        }
        internal static void GenerateInvokerMethod(CodeGenerationContext context, MethodInfo methodInfo, string contextClassName, string methodName,  Type? @interface, string? propertyName, bool isLocalMetehod)
        {
            var modifier = isLocalMetehod ? "" : "public ";
            if (@interface == null)
            {
                context.WriteLines($"{modifier}ValueTask {methodName}(InvocationContext invocationContext)");
            }
            else
            {
                context.WriteLines($"{modifier}static ValueTask {methodName}(InvocationContext invocationContext)");
            }

            var returnType = methodInfo.ReturnType;
            var parameters = methodInfo.GetParameters();
            using (context.CodeBlock())
            {
                context.WriteLines($"var context = ({contextClassName})invocationContext;");
                var target = "base.";
                if (@interface != null)
                {
                    context.WriteLines($"var target = ({@interface.GetOutputName()})invocationContext.Target;");
                    target = "target.";
                }

                IEnumerable<string>? arguments = null;
                if (string.IsNullOrEmpty(propertyName))
                {
                    arguments = from parameter in parameters
                        let name = parameter.Name
                        let isIn = parameter.IsIn
                        let isOut = parameter.IsOut
                        let isRef = parameter.ParameterType.Name.EndsWith("&")
                        select isIn ? $"in context._{name}" : isOut ? $"out context._{name}" : isRef ? $"ref context._{name}" : $"context._{name}";
                }
                if (returnType == typeof(void))
                {
                    if (arguments != null)
                    {
                        context.WriteLines($"{target}{GetNormallizedMemberName(methodInfo)}({string.Join(", ", arguments)});");
                    }
                    else
                    {
                        context.WriteLines($"{target}{propertyName} = context._value;");
                    }
                    context.WriteLines("return ValueTask.CompletedTask;");
                }
                else
                {
                    if (arguments != null)
                    {
                        context.WriteLines($"var returnValue = {target}{methodInfo.Name}({string.Join(", ", arguments)});");
                        context.WriteLines($"context._returnValue = returnValue;");
                    }
                    else
                    {
                        context.WriteLines($"context._returnValue = {target}{propertyName};");
                    }

                    if (typeof(Task).IsAssignableFrom(returnType))
                    {
                        context.WriteLines("return new ValueTask(returnValue);");
                    }
                    else if (returnType == typeof(ValueTask))
                    {
                        context.WriteLines("return returnValue;");
                    }
                    else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        context.WriteLines("return returnValue.AsValueTask();");
                    }
                    else
                    {
                        context.WriteLines("return ValueTask.CompletedTask;");
                    }
                }
            }
        }
        internal static void GenerateFields(CodeGenerationContext context, Type targetType, Type? @interface, MethodInfo[] interceptableMethods, 
            out IDictionary<MethodInfo, string> methodInfoAccessorFieldNames,
            out IDictionary<MethodInfo, string>? invokerAccessorFieldNames,
            out IDictionary<MethodInfo, string>? invokerMethodNames)
        {
            if (@interface != null)
            {
                context.WriteLines($"private readonly {@interface.GetOutputName()} _target;");
            }
            context.WriteLines($"private readonly IInvocationServiceScopeFactory _scopeFactory;");

            methodInfoAccessorFieldNames = new  Dictionary<MethodInfo, string>();
            foreach (var method in interceptableMethods)
            {
                var fieldName = ResolveMethodAccessorFieldName(method);
                context.WriteLines($"private static readonly Lazy<MethodInfo> {fieldName} = new Lazy<MethodInfo>(()=>ProxyHelper.GetMethodInfo<{targetType.GetOutputName()}>({method.MetadataToken}));");
                methodInfoAccessorFieldNames[method] = fieldName;
            }

            invokerAccessorFieldNames = null;
            invokerMethodNames = null;

            foreach (var method in interceptableMethods)
            {
                if (!method.IsGenericMethod)
                {
                    invokerAccessorFieldNames ??= new Dictionary<MethodInfo, string>();
                    invokerMethodNames ??= new Dictionary<MethodInfo, string>();
                    var fieldName = InvokerAccessorFieldName(method);
                    var methodName = ResolveInvokerMethodName(method);
                    if (@interface != null)
                    {
                        context.WriteLines($"private static readonly Lazy<InvokeDelegate> {fieldName} = new Lazy<InvokeDelegate>(() => MethodInvokerBuilder.Instance.Build(typeof({targetType.GetOutputName()}),ProxyHelper.GetMethodInfo<{targetType.GetOutputName()}>({method.MetadataToken}), {methodName}));");
                    }
                    else
                    {
                        context.WriteLines($"private readonly Lazy<InvokeDelegate> {fieldName};");
                    }
                    invokerAccessorFieldNames[method] = fieldName;
                    invokerMethodNames[method] = methodName;
                }
            }
        }

        internal static string GenerateInvocationContextClass(CodeGenerationContext context, string className, MethodInfo methodInfo)
        {
            if (methodInfo.IsGenericMethod)
            {
                var genericParameterNames = ResolveGenericArguments(methodInfo.GetGenericArguments(), out var typeConstraints);
                className = $"{className}<{genericParameterNames}>";
                context.WriteLines($"private class {className} : InvocationContext");
                if (typeConstraints != null)
                {
                    using (context.Indent())
                    {
                        foreach (var constraint in typeConstraints)
                        {
                            context.WriteLines(constraint);
                        }
                    }
                }
            }
            else
            {
                context.WriteLines($"private class {className} : InvocationContext");
            }

            var parameters = methodInfo.GetParameters();
            var returnType = methodInfo.ReturnType;

            using (context.CodeBlock())
            {
                #region Fields
                //Parameters based fields
                foreach (var parameter in parameters)
                {
                    context.WriteLines($"internal {parameter.ParameterType.GetOutputName()} _{parameter.Name};");
                }
                //Return value based field
                if (returnType != typeof(void))
                {
                    context.WriteLines($"internal {returnType.GetOutputName()} _returnValue;");
                }
                context.WriteLines();

                //MethodInfo
                context.WriteLines("public override MethodInfo MethodInfo { get; }");

                //IServiceProvider
                context.WriteLines("public override IServiceProvider InvocationServices { get; }");
                context.WriteLines();
                #endregion

                #region Constructor
                var constructorName = className;
                if (methodInfo.IsGenericMethod)
                {
                    var index = constructorName.IndexOf('<');
                    constructorName = className[..index];
                }
                var inputParameters = string.Join(", ", parameters.Where(it => !it.IsOut).Select(it =>$"{it.ParameterType.GetOutputName()} {it.Name}")).TrimEnd();
                if (!string.IsNullOrWhiteSpace(inputParameters))
                {
                    context.WriteLines($"public {constructorName}(object target, {inputParameters}, MethodInfo method, IServiceProvider invocationServices) : base(target)");
                }
                else
                {
                    context.WriteLines($"public {constructorName}(object target, MethodInfo method, IServiceProvider invocationServices) : base(target)");
                }
                using (context.CodeBlock())
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter.IsOut)
                        {
                            continue;
                        }
                        context.WriteLines($"_{parameter.Name} = {parameter.Name};");
                    }
                    context.WriteLines("MethodInfo = method;");
                    context.WriteLines("InvocationServices = invocationServices;");
                }
                context.WriteLines();
                #endregion

                #region Methods
                //GetArgument<TArgument>(string name)
                context.WriteLines("public override TArgument GetArgument<TArgument>(string name)");
                using (context.CodeBlock())
                {
                    context.WriteLines("return name switch");
                    using (context.CodeBlock("{", "};"))
                    {
                        foreach (var parameter in parameters)
                        {
                            context.WriteLines($"\"{parameter.Name}\" => ProxyHelper.GetArgumentOrReturnValue<{parameter.ParameterType.GetOutputName()}, TArgument>(_{parameter.Name}),");
                        }
                        context.WriteLines(@"_ => throw new ArgumentException($""Invalid argument name { name}."", nameof(name))");
                    }
                }
                context.WriteLines();

                //GetArgument<TArgument>(string name)
                context.WriteLines("public override TArgument GetArgument<TArgument>(int index)");
                using (context.CodeBlock())
                {
                    context.WriteLines("return index switch");
                    using (context.CodeBlock("{", "};"))
                    {
                        var index = 0;
                        foreach (var parameter in parameters)
                        {
                            context.WriteLines($"{index++} => ProxyHelper.GetArgumentOrReturnValue<{parameter.ParameterType.GetOutputName()}, TArgument>(_{parameter.Name}),");
                        }
                        context.WriteLines("_ => throw new ArgumentOutOfRangeException(nameof(index))");
                    }
                }
                context.WriteLines();

                //SetArgument<TArgument>(string name, TArgument value)
                context.WriteLines("public override InvocationContext SetArgument<TArgument>(string name, TArgument value)");
                using (context.CodeBlock())
                {
                    context.WriteLines("return name switch");
                    using (context.CodeBlock("{", "};"))
                    {
                        foreach (var parameter in parameters)
                        {
                            context.WriteLines($"\"{parameter.Name}\" => ProxyHelper.SetArgumentOrReturnValue<{className}, {parameter.ParameterType.GetOutputName()}, TArgument>(this, value, (ctx, val) => ctx._{parameter.Name} = val),");
                        }
                        context.WriteLines(@" _ => throw new ArgumentException($""Invalid argument name { name}."", nameof(name))");
                    }
                }
                context.WriteLines();

                //SetArgument<TArgument>(int index, TArgument value)
                context.WriteLines("public override InvocationContext SetArgument<TArgument>(int index, TArgument value)");
                using (context.CodeBlock())
                {
                    context.WriteLines("return index switch");
                    using (context.CodeBlock("{", "};"))
                    {
                        var index = 0;
                        foreach (var parameter in parameters)
                        {
                            context.WriteLines($"{index++} => ProxyHelper.SetArgumentOrReturnValue<{className}, {parameter.ParameterType.GetOutputName()}, TArgument>(this, value, (ctx, val) => ctx._{parameter.Name} = val),");
                        }
                        context.WriteLines("_ => throw new ArgumentOutOfRangeException(nameof(index))");
                    }
                }
                context.WriteLines();

                //GetReturnValue
                if (returnType == typeof(void))
                {
                    context.WriteLines($"public override TReturnValue GetReturnValue<TReturnValue>() => default;");
                }
                else
                {
                    context.WriteLines($"public override TReturnValue GetReturnValue<TReturnValue>() => ProxyHelper.GetArgumentOrReturnValue<{returnType.GetOutputName()}, TReturnValue>(_returnValue);");
                }

                //SetReturnValue
                if (returnType == typeof(void))
                {
                    context.WriteLines($"public override InvocationContext SetReturnValue<TReturnValue>(TReturnValue value) => this;");
                }
                else
                {
                    context.WriteLines($"public override InvocationContext SetReturnValue<TReturnValue>(TReturnValue value) => ProxyHelper.SetArgumentOrReturnValue<{className}, {returnType.GetOutputName()}, TReturnValue>(this, value, (ctx, val) => ctx._returnValue = val);");
                }
                #endregion
            }

            return className;
        }

        /// <summary>
        /// Generates the property based source code.
        /// </summary>
        /// <param name="context">The code generation context.</param>
        /// <param name="property">The target property.</param>
        /// <param name="getContextClassName">Name of the get-method specific context class.</param>
        /// <param name="setContextClassName">Name of the set-method specific context class.</param>
        /// <param name="getMethodAccessor">The get-method accessor.</param>
        /// <param name="setMethodAccessor">The set-method accessor.</param>
        /// <param name="getInvokerFieldName">Name of the get-method specific invoker field.</param>
        /// <param name="setInvokerFieldName">Name of the set-method specific invoker field.</param>
        /// <param name="interface">The interface.</param>
        internal protected static void GenerateProperty(CodeGenerationContext context, PropertyInfo property, string? getContextClassName, string? setContextClassName, string? getMethodAccessor, string? setMethodAccessor, string? getInvokerFieldName, string? setInvokerFieldName, Type? @interface)
        {
            var propertyType = property.PropertyType;
            var modifier = @interface != null ? "public" : property.GetModifier();
            var @override = @interface == null ? " override " : " ";
            context.WriteLines($"{modifier}{@override}{propertyType.GetOutputName()} {property.Name}");
            var target = @interface == null ? "this" : "_target";
            using (context.CodeBlock())
            {
                if (property.GetMethod != null)
                {
                    var methodModifier = property.GetMethod.GetModifier();
                    if (string.IsNullOrEmpty(getContextClassName))
                    {
                        if (methodModifier == modifier)
                        {
                            if (@interface != null)
                            {
                                context.WriteLines($"get => _target.{property.Name};");
                            }
                            else
                            {
                                context.WriteLines($"get => base.{property.Name};");
                            }
                        }
                        else
                        {
                            if (@interface != null)
                            {
                                context.WriteLines($"{methodModifier} get => _target.{property.Name};");
                            }
                            else
                            {
                                context.WriteLines($"{methodModifier} get => base.{property.Name};");
                            }
                        }                        
                    }
                    else
                    {
                        if (methodModifier == modifier)
                        {
                            context.WriteLines($"get");
                        }
                        else
                        {
                            context.WriteLines($"{methodModifier} get");
                        }

                        using (context.CodeBlock())
                        {
                            context.WriteLines("using var scope = _scopeFactory.CreateInvocationScope();");
                            context.WriteLines($"var context = new {getContextClassName}({target}, {getMethodAccessor}, scope.ServiceProvider);");
                            context.WriteLines($"var valueTask = {getInvokerFieldName}.Value.Invoke(context);");
                            context.WriteLines("if (!valueTask.IsCompleted) valueTask.GetAwaiter().GetResult();");
                            context.WriteLines("return context._returnValue;");
                        }
                    }
                }

                if (property.SetMethod != null)
                {
                    var methodModifier = property.SetMethod.GetModifier();
                    if (string.IsNullOrEmpty(setContextClassName))
                    {
                        if (methodModifier == modifier)
                        {
                            if (@interface != null)
                            {
                                context.WriteLines($"set => _target.{property.Name} = value;");
                            }
                            else
                            {
                                context.WriteLines($"set => base.{property.Name} = value;");
                            }
                        }
                        else
                        {
                            if (@interface != null)
                            {
                                context.WriteLines($"{methodModifier} set => _target.{property.Name} = value;");
                            }
                            else
                            {
                                context.WriteLines($"{methodModifier} set => base.{property.Name} = value;");
                            }
                        }
                    }
                    else
                    {
                        if (methodModifier == modifier)
                        {
                            context.WriteLines($"set");
                        }
                        else
                        {
                            context.WriteLines($"{methodModifier} set");
                        }

                        using (context.CodeBlock())
                        {
                            context.WriteLines("using var scope = _scopeFactory.CreateInvocationScope();");
                            context.WriteLines($"var context = new {setContextClassName}({target}, value, {setMethodAccessor}, scope.ServiceProvider);");
                            context.WriteLines($"var valueTask = {setInvokerFieldName}.Value.Invoke(context);");
                            context.WriteLines("if (!valueTask.IsCompleted) valueTask.GetAwaiter().GetResult();");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the name of the invocation context class.
        /// </summary>
        /// <param name="method">The method to intercept.</param>
        /// <returns>The concrete intocation context class name.</returns>
        internal protected static string ResolveInvocationContextClassName(MethodInfo method)
        {
            var index = method.Name.LastIndexOf('.');
            return index < 0 ? $"{method.Name}Context".AsIdentifier() : $"{method.Name[(index + 1)..]}Context".AsIdentifier();
        }

        /// <summary>
        /// Gets the name of the generated interceptable proxy class.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="typeConstraints">The type constraints.</param>
        /// <returns>The literal proxy type name.</returns>
        internal protected static string GetInterceptableProxyClassName(Type targetType, out List<string>? typeConstraints)
        {
            if (targetType.IsGenericTypeDefinition)
            {
                ResolveGenericArguments(targetType.GetGenericArguments(), out typeConstraints);
                return $"{targetType.Name.Split('`')[0]}Proxy".AsIdentifier() + $"<{string.Join(", ", targetType.GetGenericArguments().Select(it => it.GetOutputName()))}>";
            }


            typeConstraints = null;
            if (targetType.IsGenericType)
            {
                return $"{targetType.Name.Split('`')[0]}Proxy".AsIdentifier();
            }

            return $"{targetType.Name}Proxy".AsIdentifier();
        }

        internal static bool TryGetPropertyName(MethodInfo method, out string? propertyName)
        {
            if (method.IsSpecialName)
            {
                var methodName = GetNormallizedMemberName(method);
                if (methodName.StartsWith("set_") || methodName.StartsWith("get_"))
                {
                    propertyName = methodName[4..];
                    return true;
                }
            }

            return (propertyName = null) != null;
        }
        #endregion

        #region Private methods
        private static string ResolveGenericArguments(Type[] arguments, out List<string>? typeConstraints)
        {
            typeConstraints = null;
            var builder = _stringBuilderAccessor.Value!;
            foreach (var argument in arguments)
            {
                builder.Clear();
                var interfaces = argument.GetInterfaces();
                var empty = true;
                if (interfaces.Any())
                {
                    var interfaceList = string.Join(", ", interfaces.Select(it => it.GetOutputName())).Trim();
                    builder.Append($"where {argument.Name}: {interfaceList}");
                    empty = false;
                }

                var constraints = argument.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
                if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    builder.Append(empty ? $"where {argument.Name}: class" : ", class");
                    empty = false;
                }

                if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    builder.Append(empty ? $"where {argument.Name}: notnull" : ", notnull");
                    empty = false;
                }

                if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    builder.Append(empty ? $"where {argument.Name}: new()" : ", new()");
                    empty = false;
                }

                if (!empty)
                {
                    typeConstraints ??= new List<string>();
                    typeConstraints.Add(builder.ToString());
                }
            }

            var names = from argument in arguments
                        let attributes = argument.GenericParameterAttributes
                        let covariant = (attributes & GenericParameterAttributes.Covariant) != 0
                        let contravariant = (attributes & GenericParameterAttributes.Contravariant) != 0
                        select covariant ? $"out {argument.Name}" : contravariant ? $"in {argument.Name}" : argument.Name;
            return string.Join(", ", names).TrimEnd();
        }
        private static string ResolveMethodAccessorFieldName(MethodInfo method)
        {
            var index = method.Name.LastIndexOf('.');
            return index < 0 ? $"_methodOf{method.Name}".AsIdentifier() : $"_methodOf{method.Name[(index + 1)..]}".AsIdentifier();
        }
        private static string ResolveInvokerMethodName(MethodInfo method)
        {
            var index = method.Name.LastIndexOf('.');
            return index < 0 ? $"{method.Name}".AsIdentifier() : $"{method.Name[(index + 1)..]}".AsIdentifier();
        }
        private static string InvokerAccessorFieldName(MethodInfo method)
        {
            var index = method.Name.LastIndexOf('.');
            return index < 0 ? $"_invokerOf{method.Name}".AsIdentifier() : $"_methodOf{method.Name[(index + 1)..]}".AsIdentifier();
        }
        private static string GetNormallizedMemberName(MemberInfo member)
        {
            var index = member.Name.LastIndexOf('.');
            return index < 0 ? member.Name : member.Name[(index + 1)..];
        }       
        #endregion
    }
}
