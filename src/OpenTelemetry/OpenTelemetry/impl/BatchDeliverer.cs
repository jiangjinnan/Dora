namespace Dora.OpenTelemetry
{
    public class BatchDeliverer<T>
    {
        private readonly DeliverableBuffer<T> _buffer;

        public BatchDeliverer(int bufferCapacity, int batchSize, TimeSpan checkInterval, TimeSpan deliveryInterval, Action<Memory<T>?, Memory<T>?> deliver)
        {
            if (bufferCapacity <= 0) throw new ArgumentException("Argument must be positive.", nameof(bufferCapacity));
            if (batchSize <= 0) throw new ArgumentException("Argument must be positive.", nameof(batchSize));
            if (bufferCapacity < batchSize) throw new InvalidOperationException($"bufferCapacity must be greater than batchSize.");
            if(checkInterval > deliveryInterval) throw new InvalidOperationException($"deliveryInterval must be greater than checkInterval.");
            if (deliver is null) throw new ArgumentNullException(nameof(deliver));
            _buffer = new DeliverableBuffer<T>(bufferCapacity, batchSize, deliver, checkInterval, deliveryInterval);
        }            

        public bool TryEnqueue(T item)=>_buffer.TryAdd(item);
    }
}
