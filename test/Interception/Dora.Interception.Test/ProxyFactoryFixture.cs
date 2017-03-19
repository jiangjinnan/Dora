using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
  public class DynamicProxyFactoryFixture
  {
    private static string _foo;
    private static string _bar;

    [Fact]
    public void CreateProxy1()
    {
      _foo = _bar = null;
      IService svc = new ServiceCollection()
        .AddSingleton<IService, A>()
        .AddInterception(builder => builder.SetDynamicProxyFactory())
        .BuildServiceProvider()
        .ToInterceptable()
        .GetRequiredService<IService>();
      svc.M1();
      Assert.Equal("123", _foo);
      Assert.Equal("123", _bar);

      _foo = _bar = null;

      svc.M2();
      Assert.Equal("123", _foo);
      Assert.Null(_bar);
    }

    [Fact]
    public void CreateProxy2()
    {
      _foo = _bar = null;
      IService svc = new ServiceCollection()
        .AddSingleton<IService, B>()
        .AddInterception(builder => builder.SetDynamicProxyFactory())
        .BuildServiceProvider()
        .ToInterceptable()
        .GetRequiredService<IService>();
      svc.M1();

      Assert.Null(_foo);
      Assert.Equal("123", _bar);

      _foo = _bar = null;

      svc.M2();
      Assert.Null(_foo);
      Assert.Null(_bar);
    }

    public interface IService
    {
      void M1();
      void M2();
    }

    [Foo]
    [Bar]
    public class A : IService
    {
      public virtual void M1() { }

      [NonInterceptable(typeof(BarAttribute))]
      public virtual void M2() { }
    }

    [NonInterceptable(typeof(FooAttribute))]
    public class B : A
    {
    }

    private class FooAttribute : InterceptorAttribute
    {
      public override void Use(IInterceptorChainBuilder builder)
      {
        builder.Use(next => (async context => { _foo = "123"; await next(context); }), this.Order);
      }
    }

    private class BarAttribute : InterceptorAttribute
    {
      public override void Use(IInterceptorChainBuilder builder)
      {
        builder.Use(next => (async context => { _bar = "123"; await next(context); }), this.Order);
      }
    }
  }
}
