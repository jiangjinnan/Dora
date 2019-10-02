using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class ExpressionInterceptorProviderResolverFixture
    {
        [Fact]
        public async void Intercept1()
        {
            void BuildPolicy(InterceptionBuilder buidler) => buidler.AddPolicy(policy => policy
                  .For<FooInterceptorAttribute>(1, interceptor => interceptor
                      .To<FoobarService>(target => target
                          .IncludeMethod(foobar => foobar.InterceptableInvokeAsync())
                          .IncludeProperty(foobar => foobar.Both, PropertyMethod.Both)
                          .IncludeProperty(foobar => foobar.Get, PropertyMethod.Get)
                          .IncludeProperty(foobar => foobar.Set, PropertyMethod.Set))
                      .To<FoobazService>(targetBuilder => targetBuilder
                          .IncludeMethod(foobar => foobar.InterceptableInvokeAsync())
                          .IncludeProperty(foobar => foobar.Both, PropertyMethod.Both)
                          .IncludeProperty(foobar => foobar.Get, PropertyMethod.Get)
                          .IncludeProperty(foobar => foobar.Set, PropertyMethod.Set)))

                 .For<BarInterceptorAttribute>(2, interceptor => interceptor
                      .To<FoobarService>(target => target
                          .IncludeMethod(foobar => foobar.InterceptableInvokeAsync())
                          .IncludeProperty(foobar => foobar.Both, PropertyMethod.Both)
                          .IncludeProperty(foobar => foobar.Get, PropertyMethod.Get)
                          .IncludeProperty(foobar => foobar.Set, PropertyMethod.Set))
                      .To<BarbazService>(targetBuilder => targetBuilder
                          .IncludeMethod(foobar => foobar.InterceptableInvokeAsync())
                          .IncludeProperty(foobar => foobar.Both, PropertyMethod.Both)
                          .IncludeProperty(foobar => foobar.Get, PropertyMethod.Get)
                          .IncludeProperty(foobar => foobar.Set, PropertyMethod.Set)))

                  .For<BazInterceptorAttribute>(3, interceptor => interceptor
                      .To<FoobazService>(target => target
                          .IncludeAllMembers()
                          .ExcludeMethod(foobar => foobar.NonInterceptableInvokeAsync())
                          .ExcludeProperty(foobar => foobar.NonInterceptable, PropertyMethod.Both)
                          .ExcludeProperty(foobar => foobar.Get, PropertyMethod.Set)
                          .ExcludeProperty(foobar => foobar.Set, PropertyMethod.Get))
                      .To<BarbazService>(targetBuilder => targetBuilder
                          .IncludeMethod(foobar => foobar.InterceptableInvokeAsync())
                          .IncludeProperty(foobar => foobar.Both, PropertyMethod.Both)
                          .IncludeProperty(foobar => foobar.Get, PropertyMethod.Get)
                          .IncludeProperty(foobar => foobar.Set, PropertyMethod.Set))));

            async Task CheckAsync(IService svc, Type serviceType)
            {
                if (serviceType == typeof(FoobarService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }

                if (serviceType == typeof(FoobazService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    _interceptors.Clear();
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }

                if (serviceType == typeof(BarbazService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    _interceptors.Clear();
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }
            }

            var service = new ServiceCollection()
                .AddSingleton<IService, FoobarService>()
               .BuildInterceptableServiceProvider(BuildPolicy)
                .GetRequiredService<IService>();
            await CheckAsync(service, typeof(FoobarService));

            service = new ServiceCollection()
                .AddSingleton<IService, FoobazService>()
               .BuildInterceptableServiceProvider(BuildPolicy)
                .GetRequiredService<IService>();
            await CheckAsync(service, typeof(FoobazService));

            service = new ServiceCollection()
               .AddSingleton<IService, BarbazService>()
               .BuildInterceptableServiceProvider(BuildPolicy)
               .GetRequiredService<IService>();
            await CheckAsync(service, typeof(BarbazService));
        }

        [Fact]
        public async void Intercept2()
        {
            void policyBuilder(InterceptionBuilder buidler) => buidler.AddPolicy("policy.dora",policyFile => policyFile
                .AddImports("Dora.Interception.Test")
                .AddReferences(typeof(ExpressionInterceptorProviderResolverFixture).Assembly));

            async Task CheckAsync(IService svc, Type serviceType)
            {
                if (serviceType == typeof(FoobarService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BarInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }

                if (serviceType == typeof(FoobazService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    _interceptors.Clear();
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is FooInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }

                if (serviceType == typeof(BarbazService))
                {
                    _interceptors.Clear();
                    await svc.InterceptableInvokeAsync();
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    await svc.NonInterceptableInvokeAsync();
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    var value = svc.Get;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Get = null;
                    Assert.True(_interceptors.Count == 0);

                    _interceptors.Clear();
                    value = svc.Set;
                    _interceptors.Clear();
                    _interceptors.Clear();
                    svc.Set = null;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.Both;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);
                    _interceptors.Clear();
                    svc.Both = null;
                    Assert.True(_interceptors[0] is BarInterceptorAttribute);
                    Assert.True(_interceptors[1] is BazInterceptorAttribute);

                    _interceptors.Clear();
                    value = svc.NonInterceptable;
                    Assert.True(_interceptors.Count == 0);
                    _interceptors.Clear();
                    svc.NonInterceptable = null;
                    Assert.True(_interceptors.Count == 0);
                }
            }

            var service = new ServiceCollection()
                .AddSingleton<IService, FoobarService>()
               .BuildInterceptableServiceProvider(policyBuilder)
                .GetRequiredService<IService>();
            await CheckAsync(service, typeof(FoobarService));

            service = new ServiceCollection()
                .AddSingleton<IService, FoobazService>()
                .BuildInterceptableServiceProvider(policyBuilder)
                .GetRequiredService<IService>();
            await CheckAsync(service, typeof(FoobazService));

            service = new ServiceCollection()
               .AddSingleton<IService, BarbazService>()
               .BuildInterceptableServiceProvider(policyBuilder)
               .GetRequiredService<IService>();
            await CheckAsync(service, typeof(BarbazService));
        }

        internal static List<object> _interceptors = new List<object>();
    }
    public class FooInterceptorAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder) => builder.Use<FooInterceptorAttribute>(Order);
        public async Task InvokeAsync(InvocationContext context)
        {
            ExpressionInterceptorProviderResolverFixture._interceptors.Add(this);
            await context.ProceedAsync();
        }
    }
    public class BarInterceptorAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder) => builder.Use<BarInterceptorAttribute>(Order);
        public async Task InvokeAsync(InvocationContext context)
        {
            ExpressionInterceptorProviderResolverFixture._interceptors.Add(this);
            await context.ProceedAsync();
        }
    }
    public class BazInterceptorAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder) => builder.Use<BazInterceptorAttribute>(Order);
        public async Task InvokeAsync(InvocationContext context)
        {
            ExpressionInterceptorProviderResolverFixture._interceptors.Add(this);
            await context.ProceedAsync();
        }
    }

    public interface IService
    {
        Task InterceptableInvokeAsync();
        Task NonInterceptableInvokeAsync();

        object Both { get; set; }
        object Get { get; set; }
        object Set { get; set; }
        object NonInterceptable { get; set; }
    }
    public abstract class ServiceBase : IService
    {
        public object Both { get; set; }
        public object Get { get; set; }
        public object Set { get; set; }
        public object NonInterceptable { get; set; }
        public Task InterceptableInvokeAsync() => Task.CompletedTask;
        public Task NonInterceptableInvokeAsync() => Task.CompletedTask;
    }
    public class FoobarService : ServiceBase { }
    public class FoobazService : ServiceBase { }
    public class BarbazService : ServiceBase { }
}
