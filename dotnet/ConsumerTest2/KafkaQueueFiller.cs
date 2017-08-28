using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConsumerTest;
using KafkaClient;

namespace ConsumerTest2
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaSetting kafkaSetting;

        static KafkaProducerProvider()
        {
            kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri(Program.KafkaUri))
                .SetAcks(1)
                .SetRetries(0).SetLinger(TimeSpan.FromMilliseconds(20))
                .Set("socket.blocking.max.ms", 25)
                .Set("batch.num.messages", 64 * 1000)
                .Set("message.max.bytes", 20 * 1000 * 1000)
                .Set("queue.buffering.max.messages", 10000000)
                .Set("queue.buffering.max.kbytes", 2097151)
                .SetClientId("client-id")
                .SetGroupId("test-group");
        }

        public static KafkaProducer Get(Action<byte[]> receiveMessageAction = null)
        {
            return new KafkaProducer(kafkaSetting, receiveMessageAction);
        }
    }

    class KafkaQueueFiller
    {
        private const int stepMilliseconds = 500;
        public const string Topic = "topic2";
        private static int requestCount;
        private static int successCount;
        private static int errorCount;
        private static KafkaProducer kafkaProducer;

        private static void OnMessageDelivered(byte [] data)
        {
            Interlocked.Increment(ref successCount);
        }

        public static void Run()
        {
            requestCount = 0;
            successCount = 0;
            errorCount = 0;
            kafkaProducer = KafkaProducerProvider.Get(OnMessageDelivered);
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
            Thread.Sleep(2000);
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
            for (var i = 0; i < 100; i++)
            {
                kafkaProducer.Produce(Topic, Guid.Empty, body);
            }
        }
    }
}
