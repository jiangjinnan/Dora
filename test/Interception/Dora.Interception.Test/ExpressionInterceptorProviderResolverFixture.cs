using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class ExpressionInterceptorProviderResolverFixture
    {
        [Fact]
        public async void Intercept()
        {
            Action<InterceptionBuilder> policyBuilder = buidler => buidler.AddPolicy(pb => pb
                 .For<FooInterceptorAttribute>(1, providerBuilder => providerBuilder
                     .To<FoobarService>(targetBuilder => targetBuilder
                         .IncludeMethod(it => it.InterceptableInvokeAsync())
                         .IncludeProperty(it => it.Both, PropertyMethod.Both)
                         .IncludeProperty(it => it.Get, PropertyMethod.Get)
                         .IncludeProperty(it => it.Set, PropertyMethod.Set))
                     .To<FoobazService>(targetBuilder => targetBuilder
                         .IncludeMethod(it => it.InterceptableInvokeAsync())
                         .IncludeProperty(it => it.Both, PropertyMethod.Both)
                         .IncludeProperty(it => it.Get, PropertyMethod.Get)
                         .IncludeProperty(it => it.Set, PropertyMethod.Set)))

                .For<BarInterceptorAttribute>(2, providerBuilder => providerBuilder
                     .To<FoobarService>(targetBuilder => targetBuilder
                         .IncludeMethod(it => it.InterceptableInvokeAsync())
                         .IncludeProperty(it => it.Both, PropertyMethod.Both)
                         .IncludeProperty(it => it.Get, PropertyMethod.Get)
                         .IncludeProperty(it => it.Set, PropertyMethod.Set))
                     .To<BarbazService>(targetBuilder => targetBuilder
                         .IncludeMethod(it => it.InterceptableInvokeAsync())
                         .IncludeProperty(it => it.Both, PropertyMethod.Both)
                         .IncludeProperty(it => it.Get, PropertyMethod.Get)
                         .IncludeProperty(it => it.Set, PropertyMethod.Set)))

                 .For<BazInterceptorAttribute>(3, providerBuilder => providerBuilder
                     .To<FoobazService>(targetBuilder => targetBuilder
                         .IncludeAllMembers()
                         .ExecludeMethod(it => it.NonInterceptableInvokeAsync())
                         .ExcludeProperty(it => it.NonInterceptable, PropertyMethod.Both)
                         .ExcludeProperty(it => it.Get, PropertyMethod.Set)
                         .ExcludeProperty(it => it.Set, PropertyMethod.Get))
                     .To<BarbazService>(targetBuilder => targetBuilder
                         .IncludeMethod(it => it.InterceptableInvokeAsync())
                         .IncludeProperty(it => it.Both, PropertyMethod.Both)
                         .IncludeProperty(it => it.Get, PropertyMethod.Get)
                         .IncludeProperty(it => it.Set, PropertyMethod.Set))));

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

        //[Fact]
        //public async void Intercept2()
        //{
        //    async Task CheckAsync(IService svc, Type serviceType)
        //    {
        //        if (serviceType == typeof(FoobarService))
        //        {
        //            _interceptors.Clear();
        //            await svc.InterceptableInvokeAsync();
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BarInterceptorAttribute);

        //            _interceptors.Clear();
        //            await svc.NonInterceptableInvokeAsync();
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            var value = svc.Get;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BarInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Get = null;
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            value = svc.Set;
        //            Assert.True(_interceptors.Count == 0);
        //            _interceptors.Clear();
        //            svc.Set = null;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BarInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.Both;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BarInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Both = null;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BarInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.NonInterceptable;
        //            Assert.True(_interceptors.Count == 0);
        //            _interceptors.Clear();
        //            svc.NonInterceptable = null;
        //            Assert.True(_interceptors.Count == 0);
        //        }

        //        if (serviceType == typeof(FoobazService))
        //        {
        //            _interceptors.Clear();
        //            await svc.InterceptableInvokeAsync();
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            await svc.NonInterceptableInvokeAsync();
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            var value = svc.Get;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Get = null;
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            value = svc.Set;
        //            _interceptors.Clear();
        //            _interceptors.Clear();
        //            svc.Set = null;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.Both;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Both = null;
        //            Assert.True(_interceptors[0] is FooInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.NonInterceptable;
        //            Assert.True(_interceptors.Count == 0);
        //            _interceptors.Clear();
        //            svc.NonInterceptable = null;
        //            Assert.True(_interceptors.Count == 0);
        //        }

        //        if (serviceType == typeof(BarbazService))
        //        {
        //            _interceptors.Clear();
        //            await svc.InterceptableInvokeAsync();
        //            Assert.True(_interceptors[0] is BarInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            await svc.NonInterceptableInvokeAsync();
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            var value = svc.Get;
        //            Assert.True(_interceptors[0] is BarInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Get = null;
        //            Assert.True(_interceptors.Count == 0);

        //            _interceptors.Clear();
        //            value = svc.Set;
        //            _interceptors.Clear();
        //            _interceptors.Clear();
        //            svc.Set = null;
        //            Assert.True(_interceptors[0] is BarInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.Both;
        //            Assert.True(_interceptors[0] is BarInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);
        //            _interceptors.Clear();
        //            svc.Both = null;
        //            Assert.True(_interceptors[0] is BarInterceptorAttribute);
        //            Assert.True(_interceptors[1] is BazInterceptorAttribute);

        //            _interceptors.Clear();
        //            value = svc.NonInterceptable;
        //            Assert.True(_interceptors.Count == 0);
        //            _interceptors.Clear();
        //            svc.NonInterceptable = null;
        //            Assert.True(_interceptors.Count == 0);
        //        }
        //    }

        //    var service = new ServiceCollection()
        //        .AddSingleton<IService, FoobarService>()
        //       .BuildInterceptableServiceProvider(@"D:\projects\dora\test\Interception\Dora.Interception.Test\policy.txt", new Assembly[] { GetType().Assembly }, new string[] { "Dora.Interception.Test" })
        //        .GetRequiredService<IService>();
        //    await CheckAsync(service, typeof(FoobarService));

        //    service = new ServiceCollection()
        //        .AddSingleton<IService, FoobazService>()
        //       .BuildInterceptableServiceProvider(@"D:\projects\dora\test\Interception\Dora.Interception.Test\policy.txt", new Assembly[] { GetType().Assembly }, new string[] { "Dora.Interception.Test" })
        //        .GetRequiredService<IService>();
        //    await CheckAsync(service, typeof(FoobazService));

        //    service = new ServiceCollection()
        //       .AddSingleton<IService, BarbazService>()
        //       .BuildInterceptableServiceProvider(@"D:\projects\dora\test\Interception\Dora.Interception.Test\policy.txt", new Assembly[] { GetType().Assembly }, new string[] { "Dora.Interception.Test" })
        //       .GetRequiredService<IService>();
        //    await CheckAsync(service, typeof(BarbazService));
        //}


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
