using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ArgumentElement
    {
        public string Name { get;  }
        public string Value { get; }

        public ArgumentElement(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
