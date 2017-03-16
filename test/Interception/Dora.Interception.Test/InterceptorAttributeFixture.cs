using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace Dora.Interception.Test
{
  public class InterceptorAttributeFixture
  {
    [Fact]
    public void AllowMultiple()
    {
      Assert.True(new FooAttribute().AllowMultiple);
      Assert.False(new BarAttribute().AllowMultiple);
    }

    [Fact]
    public void CaptureAttributes()
    {
      var attribute = new FooAttribute();
      ((IAttributeCollection)attribute).Add(new Attribute1());
      ((IAttributeCollection)attribute).AddRange(new Attribute[] { new Attribute2(), new Attribute3()});

      Assert.True(((IAttributeCollection)attribute).OfType<Attribute1>().Any());
      Assert.True(((IAttributeCollection)attribute).OfType<Attribute2>().Any());
      Assert.True(((IAttributeCollection)attribute).OfType<Attribute3>().Any());
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private class FooAttribute : InterceptorAttribute
    {
      public override void Use(IInterceptorChainBuilder builder)
      {
        throw new NotImplementedException();
      }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    private class BarAttribute : InterceptorAttribute
    {
      public override void Use(IInterceptorChainBuilder builder)
      {
        throw new NotImplementedException();
      }
    }

    private class Attribute1 : Attribute { }
    private class Attribute2 : Attribute { }
    private class Attribute3 : Attribute { }

  }
}
