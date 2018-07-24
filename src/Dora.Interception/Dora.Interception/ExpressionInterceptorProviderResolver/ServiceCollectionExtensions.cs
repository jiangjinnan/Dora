using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace Dora.Interception
{
    public static class ServiceCollectionExtensions
    {        
        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, Action<IInterceptionPolicyBuilder> configure)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var registrationBuilder = new InterceptionPolicyBuilder(serviceProvider);
            configure.Invoke(registrationBuilder);              
            var resolver = new ExpressionInterceptorProviderResolver(registrationBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(ExpressionInterceptorProviderResolver), resolver);
            return builder;
        }

        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, string fileName, Assembly[] references, string[] imports)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var policyBuilder = new InterceptionPolicyBuilder(serviceProvider);
            var script = CSharpScript
                .Create<IInterceptionPolicyBuilder>("var builder = Builder;", ScriptOptions.Default
                    .WithReferences(typeof(IInterceptionPolicyBuilder).Assembly)
                    .WithReferences(references)
                    .WithImports(imports)
                    .WithImports("Dora.Interception"), typeof(Host))
                .ContinueWith(File.ReadAllText(fileName), ScriptOptions.Default
                    .WithReferences(typeof(IInterceptionPolicyBuilder).Assembly)
                    .WithReferences(references)
                    .WithImports(imports)
                    .WithImports("Dora.Interception"))
                .ContinueWith("builder");
            script.RunAsync(new Host(policyBuilder)).Wait();
            var resolver = new ExpressionInterceptorProviderResolver(policyBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(ExpressionInterceptorProviderResolver), resolver);
            return builder;
        }
    }

    public class Host
    {
        public Host(IInterceptionPolicyBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }
        public IInterceptionPolicyBuilder Builder { get; }
    }
}
