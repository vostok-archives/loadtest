using System.Threading;
using System.Threading.Tasks;

namespace ConsumerTest
{
    public static class MetricsReporter
    {
        private static readonly object lockObject = new object();

        public static long LastThroughput { get; private set; }
        public static long LastThroughputBytes { get; private set; }
        public static double LastMeanTravelTimeMs { get; private set; }
        public static long TotalSize { get; private set; }
        public static long TotalTravelTimeMs { get; private set; }
        public static long TotalCount { get; private set; }

        static MetricsReporter()
        {
            new Task(ActualizeThrougput).Start();
        }

        public static void Add(int count, int size, long travelTimeMs)
        {
            lock (lockObject)
            {
                TotalCount += count;
                TotalSize += count * size;
                TotalTravelTimeMs += travelTimeMs;
            }
        }

        public static void ActualizeThrougput()
        {
            const int actualizePeriodInMillisecons = 500;
            var cancellationToken = new CancellationTokenSource();
            while (!cancellationToken.IsCancellationRequested)
            {
                GetValue(out var lastTotalCount, out var lastTotalSize, out var lastMeanTravelTimeMs);
                Thread.Sleep(actualizePeriodInMillisecons);
                GetValue(out var currentTotalCount, out var currentTotalSize, out var currentMeanTravelTimeMs);
                LastThroughput = (long) CalculateThroughput(currentTotalCount, lastTotalCount, actualizePeriodInMillisecons);
                LastThroughputBytes = (long) CalculateThroughput(currentTotalSize, lastTotalSize, actualizePeriodInMillisecons);
                LastMeanTravelTimeMs = CalculateThroughput(currentMeanTravelTimeMs, lastMeanTravelTimeMs, actualizePeriodInMillisecons);
            }
        }

        private static double CalculateThroughput(double current, double prev, int periodInMillisecons)
        {
            return (current - prev) * 1000 / periodInMillisecons;
        }

        private static void GetValue(out long lastTotalCount, out long lastTotalSize, out double lastTotalTravelTimeMs)
        {
            lock (lockObject)
            {
                lastTotalCount = TotalCount;
                lastTotalSize = TotalSize;
                lastTotalTravelTimeMs = TotalTravelTimeMs;
            }
        }
    }
}