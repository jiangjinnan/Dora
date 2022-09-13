using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora.OpenTelemetry
{
    public class BatchDelivererOptions
    {
        public int BatchSize { get; set; } = 100;
        public TimeSpan Internal { get; set; } = TimeSpan.FromSeconds(10);
    }
}
