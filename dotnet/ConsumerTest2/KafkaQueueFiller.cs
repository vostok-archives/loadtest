using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KafkaClient;

namespace ConsumerTest
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaProducer kafkaProducer;

        static KafkaProducerProvider()
        {
            kafkaProducer = new KafkaProducer(Program.KafkaSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }

    class KafkaQueueFiller
    {
        private const int stepMilliseconds = 500;
        private static int requestCount;
        private static int successCount;
        private static int errorCount;
        private static KafkaProducer kafkaProducer;

        public static void Run()
        {
            kafkaProducer = KafkaProducerProvider.Get();
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8888") };

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var tasks = new List<Task>();
            var watcherTask = new Task(() =>
            {
                var counter = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var prevSuccess = successCount;
                    Thread.Sleep(stepMilliseconds);
                    counter++;
                    var newSuccess = successCount;
                    var rps = (double)(newSuccess - prevSuccess) / stepMilliseconds * 1000;
                    var avgRps = (double)successCount / counter / stepMilliseconds * 1000;
                    Program.Log($"tasks= {tasks.Count}, success = {successCount}, error = {errorCount}, perSecond={rps}, avg={avgRps}");
                }
            }, cancellationToken, TaskCreationOptions.LongRunning);
            watcherTask.Start();

            for (var i = 0; i < 1; i++)
            {
                for (var j = 0; j < 2000; j++)
                {
                    var task = new Task(() =>
                    {
                        SendingLoop(httpClient, cancellationToken);
                    }, cancellationToken);
                    task.Start();
                    tasks.Add(task);
                }
            }
            Thread.Sleep(20000);
            cancellationTokenSource.Cancel();

            Program.Log($"success = {successCount}, all = {requestCount}");
        }

        private static readonly byte[] body = Enumerable.Range(0, 10)
            .Select(i => new Random().Next(256))
            .Select(@int => (byte)@int)
            .ToArray();
        private static void SendingLoop(HttpClient httpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref requestCount);
                try
                {
                    Produce(cancellationToken);
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
        }

        private static void Produce(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(kafkaProducer.ProduceAsync("topic", Guid.NewGuid(), body));
                Interlocked.Increment(ref successCount);
            }
            Task.WaitAll(tasks.ToArray(), cancellationToken);
        }
    }
}
