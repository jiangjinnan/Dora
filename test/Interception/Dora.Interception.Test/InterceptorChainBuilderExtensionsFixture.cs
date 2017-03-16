using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Dora;
using System.Threading.Tasks;
using System.Reflection;

namespace Dora.Interception.Test
{
  public class InterceptorChainBuilderExtensionsFixture
  {
    private static Action _intercept;

    [Theory]
    [InlineData(null, "1")]
    [InlineData("1;", null)]
    public void Use_Arguments_Not_Allow_Null(string builderIndicator, string typeIndicator)
    {
      IInterceptorChainBuilder builder = builderIndicator == null ? null : new InterceptorChainBuilder(new ServiceCollection().BuildServiceProvider());
      Type type = typeIndicator == null ? null : typeof(string);
      Assert.Throws<ArgumentNullException>(() => builder.Use(type, 1));
    }

    [Fact]
    public async void Use()
    {
      var provider = new ServiceCollection().AddScoped<IService, Service>().BuildServiceProvider();
      var builder = new InterceptorChainBuilder(provider);
      string value = null;
      _intercept = () => value = Guid.NewGuid().ToString();
      var interceptorPipeline = builder.Use<FoobarInterceptor>(1, "abc").Build();
      await interceptorPipeline(context => Task.CompletedTask)(new FoobarInvocationContext());
      Assert.NotNull(value);
    }

    private class FoobarInterceptor
    {
      private InterceptDelegate _next;
      public FoobarInterceptor(InterceptDelegate next, IService service, string argument)
      {
        _next = next;
        if (null == service || null == argument)
        {
          throw new InvalidOperationException();
        }
      }

      public async Task InvokeAsync(InvocationContext context)
      {
        _intercept();
        await _next(context);
      }
    }
    private interface IService { }
    private class Service : IService { }
    private class FoobarInvocationContext : InvocationContext
    {
      public override object[] Arguments => throw new NotImplementedException();

      public override Type[] GenericArguments => throw new NotImplementedException();

      public override object InvocationTarget => throw new NotImplementedException();

      public override MethodInfo Method => throw new NotImplementedException();

      public override MethodInfo MethodInvocationTarget => throw new NotImplementedException();

      public override object Proxy => throw new NotImplementedException();

      public override object ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public override Type TargetType => throw new NotImplementedException();

      public override object GetArgumentValue(int index)
      {
        throw new NotImplementedException();
      }

      public override void SetArgumentValue(int index, object value)
      {
        throw new NotImplementedException();
      }
    }
  }
}
