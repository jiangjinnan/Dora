using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class FilterConfiguration
    {
        public Type FilterType { get; }
        public IList<ArgumentConfiguration> Arguments { get; }
        public FilterConfiguration(Type filterType)
        {
            this.FilterType = filterType;
            this.Arguments = new List<ArgumentConfiguration>();
        }
    }
}
