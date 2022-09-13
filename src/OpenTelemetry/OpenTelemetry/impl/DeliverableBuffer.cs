namespace Dora.OpenTelemetry
{
    internal class DeliverableBuffer<T>
    {
        private readonly T[] _array;
        private volatile int _readIndex;
        private volatile int _writeIndex;
        private readonly int _capacity;
        private readonly int _batchSize;
        private volatile int _count;
        private readonly int _length;
        private readonly Action<Memory<T>?, Memory<T>?> _deliver;
        private DateTimeOffset _lastFlushTime;


        public DeliverableBuffer(int capacity, int batchSize, Action<Memory<T>?, Memory<T>?> deliver, TimeSpan checkInterval, TimeSpan deliveryInterval)
        {
            _capacity = capacity;
            _batchSize = batchSize;
            _deliver = deliver;
            _array = new T[_capacity];
            _readIndex = 0;
            _writeIndex = 0;
            _count = 0;
            _length = capacity;
            _lastFlushTime = DateTimeOffset.MinValue;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_count >= _batchSize || (DateTimeOffset.UtcNow - _lastFlushTime >= checkInterval && _count > 0))
                    {
                        _lastFlushTime = DateTimeOffset.UtcNow;
                        Deliver();
                        continue;
                    }
                    else
                    {
                        var delay = checkInterval - (DateTimeOffset.UtcNow - _lastFlushTime);
                        if (delay > TimeSpan.Zero)
                        {
                            Task.Delay(delay).Wait();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public bool TryAdd(T item)
        {
            if (_count >= _capacity)
            {
                return false;
            }

            int index = _writeIndex;

            //hit the tail
            if (index == _length)
            {
                Interlocked.CompareExchange(ref _writeIndex, 0, _length);
                return TryAdd(item);
            }

            //not get lock
            if (Interlocked.CompareExchange(ref _writeIndex, index + 1, index) != index)
            {
                return TryAdd(item);
            }

            _array[index] = item;
            Interlocked.Increment(ref _count);
            return true;
        }

        private void Deliver()
        {
            var start = _readIndex;
            var end = _writeIndex;

            if (end > start || (end <= start) && _length - start >= _batchSize)
            {
                int count;
                if (end > start)
                {
                    count = Math.Min(end - start, _batchSize);
                }
                else
                {
                    count = Math.Min(_length - start, _batchSize);
                }
                var items = _array.AsMemory(start, count);
                _deliver(items, default);
                Interlocked.Add(ref _readIndex, count);
                Interlocked.Add(ref _count, count * -1);
                Interlocked.CompareExchange(ref _readIndex, _length, 0);
                return;
            }

            var count1 = _length - start;
            var items1 = count1 == 0 ? null : _array.AsMemory(start, count1);
            var count2 = Math.Min(_batchSize - count1, _writeIndex);
            var items2 = _array.AsMemory(0, count2);

            _deliver(items1, items2);
            Interlocked.Exchange(ref _readIndex, count2);
            Interlocked.Add(ref _count, (count1 + count2) * -1);
            Interlocked.CompareExchange(ref _readIndex, _length, 0);
        }
    }
}