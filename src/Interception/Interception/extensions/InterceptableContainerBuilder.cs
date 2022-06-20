using Dora.Interception;
using Dora.Interception.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Dora;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder used to build interceptable <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class InterceptableContainerBuilder
    {
        #region Fields
        private readonly IServiceCollection _services;
        private readonly ServiceProviderOptions _serviceProviderOptions;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptableContainerBuilder"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing the orginal service registrations.</param>
        /// <param name="serviceProviderOptions">The options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.</param>
        /// <param name="setup">The <see cref="Action{InterceptionBuilder}"/> used for advanced servide registration.</param>
        public InterceptableContainerBuilder(IServiceCollection services, ServiceProviderOptions serviceProviderOptions, Action<InterceptionBuilder>? setup)
        {
            _services = Guard.ArgumentNotNull(services);
            services.AddInterception(setup);
            services.AddSingleton<IServiceLifetimeProvider>(new ServiceLifetimeProvider(services));
            _serviceProviderOptions = serviceProviderOptions ?? throw new ArgumentNullException(nameof(serviceProviderOptions));           
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates the service provider.
        /// </summary>
        /// <returns>The created <see cref="IServiceProvider"/>.</returns>
        public IServiceProvider CreateServiceProvider()
        {
            var provider = _services.BuildServiceProvider();
            try
            {
                var applicationServiceAccessor = provider.GetRequiredService<IApplicationServicesAccessor>();
               ((ApplicationServicesAccessor) applicationServiceAccessor).ApplicationServices = provider;
                MethodInvokerBuilder.Instance = provider.GetRequiredService<IMethodInvokerBuilder>();
                var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Dora.Interception");
                var log4GenerateCode = LoggerMessage.Define<string>(LogLevel.Information, 0, "Interceptable proxy classes are generated. " + Environment.NewLine + Environment.NewLine + "{0}");
                var codeGenerators = provider.GetServices<ICodeGenerator>();
                return CreateServiceProviderCore(codeGenerators, logger, log4GenerateCode);
            }
            finally
            { 
                (provider as IDisposable)?.Dispose();
            }
        }
        #endregion

        #region Private methods
        private IServiceProvider CreateServiceProviderCore(IEnumerable<ICodeGenerator> codeGenerators, ILogger logger, Action<ILogger, string, Exception> log4GenerateCode)
        {
            var generatedTypes = new List<GeneratedTypeEntry>();
            var generationContext = new CodeGenerationContext();
            generationContext.WriteLines("using System;");
            generationContext.WriteLines("using System.Reflection;");
            generationContext.WriteLines("using System.Threading.Tasks;");
            generationContext.WriteLines("using Microsoft.Extensions.DependencyInjection;");
            generationContext.WriteLines("");

            generationContext.WriteLines("namespace Dora.Interception.CodeGeneration");
            using (generationContext.CodeBlock())
            {
                foreach (var service in _services)
                {
                    foreach (var generator in codeGenerators)
                    {
                        if (generator.TryGenerate(service, generationContext, out var proxyTypeNames))
                        {
                            generatedTypes.Add(new GeneratedTypeEntry(service, proxyTypeNames!, generator));
                            break;
                        }
                    }
                }
            }

            log4GenerateCode(logger, generationContext.SourceCode, null!);

            if (generatedTypes.Any())
            {
                var compilation = CSharpCompilation.Create("Dora.Interception.CodeGeneration")
                 .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release))
                 .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(generationContext.SourceCode))
                 .AddReferences(generationContext.References.Select(it => MetadataReference.CreateFromFile(it.Location)));

                Assembly outputAssembly;
                using var stream = new MemoryStream();
                var compilationResult = compilation.Emit(stream);

                if (!compilationResult.Success)
                {
                    var error = string.Join(Environment.NewLine, compilationResult.Diagnostics);
                    throw new InterceptionException($"It fails to generate proxy class. \n {error}");
                }

                var bytes = stream.ToArray();
                outputAssembly = Assembly.Load(bytes);

                foreach (var entry in generatedTypes)
                {
                    var proxyTypes = entry.ProxyTypeNames.Select(it => outputAssembly.GetType(it)).ToArray();
                    entry.CodeGenerator.RegisterProxyType(_services, entry.ServiceDescriptor, proxyTypes!);
                }
            }
            _services.Replace(ServiceDescriptor.Singleton<IServiceLifetimeProvider> (new ServiceLifetimeProvider(_services)));
            var serviceProvider = _services.BuildServiceProvider(_serviceProviderOptions);
            ((ApplicationServicesAccessor)serviceProvider.GetRequiredService<IApplicationServicesAccessor>()).ApplicationServices = serviceProvider;
            MethodInvokerBuilder.Instance = serviceProvider.GetRequiredService<IMethodInvokerBuilder>();
            return serviceProvider;
        }

        private class GeneratedTypeEntry
        {
            public ServiceDescriptor ServiceDescriptor { get; }
            public string[] ProxyTypeNames { get; }
            public ICodeGenerator CodeGenerator { get; }
            public GeneratedTypeEntry(ServiceDescriptor serviceDescriptor, string[] proxyTypeNames, ICodeGenerator codeGenerator)
            {
                ServiceDescriptor = serviceDescriptor;
                ProxyTypeNames = proxyTypeNames;
                CodeGenerator = codeGenerator;
            }
        }
        #endregion
    }
}
