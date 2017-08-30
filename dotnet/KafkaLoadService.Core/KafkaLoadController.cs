using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KafkaClient;
using Microsoft.AspNetCore.Mvc;

namespace KafkaLoadService.Core
{
    public class KafkaLoadController : ControllerBase
    {
        private const string TopicName = "topic-kload-dot-net";
        private readonly KafkaProducer kafkaProducer;

        public KafkaLoadController()
        {
            kafkaProducer = KafkaProducerProvider.Get();
        }

        [HttpGet]
        public string Ping()
        {
            return "Ok";
        }

        [HttpGet]
        public async Task LoadWithTimerAsync(int requestCount, int bodySize)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await LoadAsync(requestCount, bodySize).ConfigureAwait(false);
            stopwatch.Stop();
            Console.WriteLine(
                $"{nameof(requestCount)}: {requestCount}, {nameof(bodySize)}: {bodySize}, ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}");
        }

        [HttpGet]
        public Task Load10Async() => LoadAsync(100, 10);
        [HttpGet]
        public Task Load100Async() => LoadAsync(100, 100);
        [HttpGet]
        public Task Load1000Async() => LoadAsync(100, 1000);
        [HttpGet]
        public Task GenerateAsync() => LoadAsync(100, 10, false);

        [HttpGet]
        public Task LoadAsync(int requestCount, int bodySize) => LoadAsync(requestCount, bodySize, true);

        private async Task LoadAsync(int requestCount, int bodySize, bool publishToKafka)
        {
            var random = new Random();
            var bodies = Enumerable.Range(0, requestCount)
                .Select(i => GenerateBody(random, bodySize))
                .ToArray();
            if (publishToKafka)
            {
                foreach (var body in bodies)
                {
                    kafkaProducer.Produce(TopicName, Guid.Empty, body);
                }
            }
            MetricsReporter.Produced(requestCount, bodySize);
        }

        private static byte[] GenerateBody(Random random, int bodySize)
        {
            return Enumerable.Range(0, bodySize)
                .Select(i => random.Next(256))
                .Select(@int => (byte)@int)
                .ToArray();
        }

        [HttpGet]
        public void Error()
        {
            throw new Exception();
        }
    }
}