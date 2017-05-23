using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ArgumentConfiguration
    {
        public string Name { get;  }
        public string Value { get; }

        public ArgumentConfiguration(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
