using System;
using System.Collections.Generic;
using System.Text;
using Dora.ExceptionHandling.Configuration;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ConfigurationBuilderFixture
    {
        [Fact]
        public void Build()
        {
            //var builder = new ConfigurationBuilder(new PhysicalFileProvider(Directory.GetCurrentDirectory()), "exception.json");
            //var policies = builder.Build();
        }
    }
}
