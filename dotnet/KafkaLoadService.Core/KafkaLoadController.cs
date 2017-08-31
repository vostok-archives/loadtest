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
        private const string TopicName = "dot-net";
        private readonly KafkaProducer kafkaProducer;
        private static readonly Random random = new Random();

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
        public async Task Load10Async() => await LoadAsync(100, 10);

        [HttpGet]
        public async Task Load100Async() => await LoadAsync(100, 100);
        [HttpGet]
        public async Task Load1000Async() => await LoadAsync(100, 1000);
        [HttpGet]
        public async Task GenerateAsync() => await LoadAsync(100, 10, false);

        [HttpGet]
        public async Task LoadAsync(int requestCount, int bodySize) => await LoadAsync(requestCount, bodySize, true);

        private async Task LoadAsync(int requestCount, int bodySize, bool publishToKafka)
        {
            if (publishToKafka)
            {
                for (var i = 0; i < requestCount; i++)
                {
                    var body = new byte[bodySize];
                    random.NextBytes(body);
                    await kafkaProducer.ProduceAsync(TopicName, Guid.NewGuid(), body);
                }
            }
            MetricsReporter.Produced(requestCount, bodySize);
        }

        [HttpGet]
        public void Error()
        {
            throw new Exception();
        }
    }
}