using System.Buffers;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public struct PooledActivityCollection: IDisposable
    {
        public ArraySegment<Activity> Activities { get; }
        public PooledActivityCollection(int batchSize, Memory<Activity>? firstBatch, Memory<Activity>? secondBatch)
        {
            var array = ArrayPool<Activity>.Shared.Rent(batchSize);
            firstBatch?.CopyTo(array.AsMemory(0));
            secondBatch?.CopyTo(array.AsMemory(firstBatch!.Value.Length));
            Activities = new ArraySegment<Activity>(array, 0, firstBatch?.Length??0 + (secondBatch?.Length ?? 0));
        }

        public void Dispose()
        {
            var array = Activities.Array;
            if (null != array)
            {
                ArrayPool<Activity>.Shared.Return(array);
            }
        }
    }
}
