namespace Dora.OpenTelemetry.Zipkin
{
    internal static class DateTimeExtensions
    {
        private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
        private static readonly long UnixEpochTicks = DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks;//621355968000000000L
        private static readonly long UnixEpochMicroseconds = UnixEpochTicks / TicksPerMicrosecond;
        internal static long AsEpochMicroseconds(this DateTime utcDateTime)
        {
            long microseconds = utcDateTime.Ticks / TicksPerMicrosecond;
            return microseconds - UnixEpochMicroseconds;
        }

        internal static long AsEpochMicroseconds(this TimeSpan timeSpan)
        {
            return timeSpan.Ticks / TicksPerMicrosecond;
        }
    }
}
