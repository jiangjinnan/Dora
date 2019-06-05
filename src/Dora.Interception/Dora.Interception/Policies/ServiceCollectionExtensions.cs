using Dora.Interception.Policies;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    /// <summary>
    /// Define extension methods agasint <see cref="InterceptionBuilder"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the interception policy.
        /// </summary>
        /// <param name="builder">The <see cref="InterceptionBuilder"/> to perform interception service registration.</param>
        /// <param name="configure">The <see cref="Action{IInterceptionPolicyBuilder}"/> used to build interception policy.</param>
        /// <returns>The current <see cref="InterceptionBuilder"/>. </returns>
        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, Action<IInterceptionPolicyBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configure, nameof(configure));

            var serviceProvider = builder.Services.BuildServiceProvider();
            var registrationBuilder = new InterceptionPolicyBuilder(serviceProvider);
            configure.Invoke(registrationBuilder);              
            var resolver = new PolicyInterceptorProviderResolver(registrationBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(PolicyInterceptorProviderResolver), resolver);
            return builder;
        }

        /// <summary>
        /// Adds the policy.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, string fileName, Action<PolicyFileBuilder> configure = null)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var fileBuilder = new PolicyFileBuilder()
                .AddImports("Dora.Interception", "System")
                .AddReferences(typeof(InterceptionPolicyBuilder).Assembly);
            configure?.Invoke(fileBuilder);
            var options = ScriptOptions.Default
                    .WithReferences(fileBuilder.References)
                    .WithImports(fileBuilder.Imports);
            var policyBuilder = new InterceptionPolicyBuilder(serviceProvider);
            var script = CSharpScript
                .Create($"var builder = Builder;{fileBuilder.ReadAllText(fileName)}", options, typeof(Host));            
            script.RunAsync(new Host(policyBuilder)).Wait();
            var resolver = new PolicyInterceptorProviderResolver(policyBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(PolicyInterceptorProviderResolver), resolver);
            return builder;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PolicyFileBuilder
    {
        private IFileProvider _fileProvider;
        private readonly HashSet<Assembly> _references = new HashSet<Assembly>();
        private readonly HashSet<string> _imports = new HashSet<string>();

        /// <summary>
        /// Sets the file provider.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <returns></returns>
        public PolicyFileBuilder SetFileProvider(IFileProvider fileProvider)
        {
            _fileProvider = Guard.ArgumentNotNull(fileProvider, nameof(fileProvider));
            return this;
        }

        /// <summary>
        /// Adds the references.
        /// </summary>
        /// <param name="references">The references.</param>
        /// <returns></returns>
        public PolicyFileBuilder AddReferences(params Assembly[] references)
        {
            Array.ForEach(references, it => _references.Add(it));
            return this;
        }

        /// <summary>
        /// Adds the imports.
        /// </summary>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns></returns>
        public PolicyFileBuilder AddImports(params string[] namespaces)
        {
            Array.ForEach(namespaces, it => _imports.Add(it));
            return this;
        }

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        /// <value>
        /// The file provider.
        /// </value>
        public IFileProvider FileProvider => _fileProvider ?? new PhysicalFileProvider(Directory.GetCurrentDirectory());

        /// <summary>
        /// Gets the references.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public Assembly[] References => _references.ToArray();

        /// <summary>
        /// Gets the imports.
        /// </summary>
        /// <value>
        /// The imports.
        /// </value>
        public string[] Imports => _imports.ToArray();

        /// <summary>
        /// Reads all text.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public string ReadAllText(string fileName)
        {
            byte[] buffer;
            using (var stream = FileProvider.GetFileInfo(fileName).CreateReadStream())
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);                 
            }
            return Encoding.Default.GetString(buffer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Host
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Host"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public Host(IInterceptionPolicyBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IInterceptionPolicyBuilder Builder { get; }
    }
}
