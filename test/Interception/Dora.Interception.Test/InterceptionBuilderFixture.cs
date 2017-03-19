using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.Interception.Test
{
  public class InterceptionBuilderFixture
  {
    [Fact]
    public void Construct_Arguments_Not_Allow_Null()
    {
      Assert.Throws<ArgumentNullException>(() => new InterceptionBuilder(null));
    }

    [Fact]
    public void Construct()
    {
      var services = new ServiceCollection();
      Assert.Same(services, new InterceptionBuilder(services).Services);
    }
  }
}
