using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class HandlerSection
    {
        public string HandlerType { get; set; }
        public IEnumerable<ArgumentSection> Arguments { get; set; }
    }
}
