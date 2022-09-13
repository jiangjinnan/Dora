using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    internal class ConsoleExporter : IActivityExporter
    {
        public void Export(IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                Console.WriteLine($"{activity.SpanId}|{activity.ParentSpanId}|{activity.OperationName}");
            }

            Console.WriteLine();
        }
    }
}
