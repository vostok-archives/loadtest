using System;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaLoadService.Core
{
    public static class MetricsReporter
    {
        private static long totalCount;
        private static long totalSize;
        private static long totalErrors;
        private static readonly object lockObject = new object();

        public static long LastThroughput { get; private set; }
        public static long LastThroughputBytes { get; private set; }

        static MetricsReporter()
        {
            new Task(ActualizeThrougput).Start();
        }

        public static void Error(int errors)
        {
            Interlocked.Add(ref totalErrors, errors);
        }
        public static void Produced(int count, int size)
        {
            Interlocked.Add(ref totalCount, count);
            Interlocked.Add(ref totalSize, count * size);
            //lock (lockObject)
            //{
            //    totalCount += count;
            //    totalSize += count * size;
            //}
        }

        public static void ActualizeThrougput()
        {
            const int actualizePeriodInMillisecons = 500;
            var cancellationToken = new CancellationTokenSource();
            while (!cancellationToken.IsCancellationRequested)
            {
                GetValue(out var lastTotalCount, out var lastTotalSize);
                Thread.Sleep(actualizePeriodInMillisecons);
                GetValue(out var currentTotalCount, out var currentTotalSize);
                LastThroughput = CalculateThroughput(currentTotalCount, lastTotalCount, actualizePeriodInMillisecons);
                LastThroughputBytes = CalculateThroughput(currentTotalSize, lastTotalSize, actualizePeriodInMillisecons);
                Console.WriteLine($"Throughput: {LastThroughput}, ThroughputSize: {LastThroughputBytes}, Errors: {totalErrors}, TotalCount:{currentTotalCount}, TotalSize:{currentTotalSize}");
            }
        }

        private static long CalculateThroughput(long current, long prev, int periodInMillisecons)
        {
            return (current - prev) * 1000 / periodInMillisecons;
        }

        private static void GetValue(out long lastTotalCount, out long lastTotalSize)
        {
            //lock (lockObject)
            //{
                lastTotalCount = totalCount;
                lastTotalSize = totalSize;
            //}
        }
    }
}