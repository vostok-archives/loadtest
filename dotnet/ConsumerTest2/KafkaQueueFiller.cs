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
            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri("http://localhost:9092"))
                .SetAcks(1)
                .SetRetries(0).SetLinger(TimeSpan.FromMilliseconds(20))
                .SetCompression(CompressionCodes.none)
                //.Set("linger.ms", 20)
                //.Set("batch.size", 64 * 1000)
                //.Set("buffer.memory", 256 * 1000 * 1000)
                //.Set("max.request.size", 20 * 1000 * 1000)
                //.Set("compression.type", "none")
                //.Set("metadata.fetch.timeout.ms", 25)
                //.Set("max.block.ms", 25)
                //.Set("max.in.flight.requests.per.connection", 500)
                //.Set("queue.buffering.max.ms", 200)

                .Set("socket.blocking.max.ms", 25)
                .Set("batch.num.messages", 64 * 1000)
                .Set("message.max.bytes", 20 * 1000 * 1000)
                .Set("queue.buffering.max.messages", 10000000)
                .Set("queue.buffering.max.kbytes", 2097151)
                .SetClientId("client-id")
                .SetGroupId("test-group");
            kafkaProducer = new KafkaProducer(kafkaSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }

    class KafkaQueueFiller
    {
        private const int stepMilliseconds = 500;
        private static int requestCount = 0;
        private static int successCount = 0;
        private static int errorCount = 0;
        static int lastSuccessPerSecond = 0;
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
                    Program.Logger.Info($"tasks= {tasks.Count}, success = {successCount}, error = {errorCount}, perSecond={rps}, avg={avgRps}");
                }
            }, cancellationToken, TaskCreationOptions.LongRunning);
            watcherTask.Start();

            for (var i = 0; i < 1; i++)
            {
                for (var j = 0; j < 2000; j++)
                {
                    var task = new Task(async () =>
                    {
                        await SendingLoop(httpClient, cancellationToken);
                    });
                    task.Start();
                    tasks.Add(task);
                }
            }
            Thread.Sleep(60000);
            cancellationTokenSource.Cancel();

            Program.Logger.Info($"success = {successCount}, all = {requestCount}");
        }

        private static readonly byte[] body = Enumerable.Range(0, 10)
            .Select(i => new Random().Next(256))
            .Select(@int => (byte)@int)
            .ToArray();
        private static async Task SendingLoop(HttpClient httpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref requestCount);
                try
                {
                    for (var i = 0; i < 100; i++)
                    {
                        await kafkaProducer.ProduceAsync("topic", Guid.NewGuid(), body);
                        Interlocked.Increment(ref successCount);
                    }
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
        }
    }
}
