using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KafkaClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace KafkaLoadService.Core
{
    public class KafkaLoadController : ControllerBase
    {
        private const string TopicName = "dot-net-next";
        private readonly KafkaProducer kafkaProducer;
        private static readonly Random random = new Random();
        private static readonly byte[] bigRandomBuffer = new byte[short.MaxValue*100 - 5];

        public KafkaLoadController()
        {
            kafkaProducer = KafkaProducerProvider.Get();
        }

        static KafkaLoadController()
        {
            random.NextBytes(bigRandomBuffer);
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
        public void Load10() => Load(100, 10);
        [HttpGet]
        public async Task Load100Async() => await LoadAsync(100, 100);
        [HttpGet]
        public void Load100() => Load(100, 100);
        [HttpGet]
        public async Task Load1000Async() => await LoadAsync(100, 1000);
        [HttpGet]
        public void Load1000() => Load(100, 1000);
        [HttpGet]
        public async Task GenerateAsync() => await LoadAsync(100, 10, false);

        [HttpGet]
        public async Task LoadAsync(int requestCount, int bodySize) => await LoadAsync(requestCount, bodySize, true);
        [HttpGet]
        public void Load(int requestCount, int bodySize) => Load(requestCount, bodySize, true);

        private async Task LoadAsync(int requestCount, int bodySize, bool publishToKafka)
        {
            if (publishToKafka)
            {
                if (SettingsProvider.GetSettings().MergeMessages)
                {
                    var body = GetRandomBody(bodySize * requestCount);
                    await kafkaProducer.ProduceAsync(TopicName, Guid.NewGuid(), body);
                }
                else
                {
                    var tasks = new List<Task>();
                    for (var i = 0; i < requestCount; i++)
                    {
                        var body = GetRandomBody(bodySize);
                        random.NextBytes(body);
                        tasks.Add(kafkaProducer.ProduceAsync(TopicName, Guid.NewGuid(), body));
                    }
                    await Task.WhenAll(tasks.ToArray());
                }
            }
            MetricsReporter.Produced(requestCount, bodySize);
        }

        private static byte[] GetRandomBody(int size)
        {
            var body = new byte[size];
            var offset = random.Next(bigRandomBuffer.Length - size);
            Array.Copy(bigRandomBuffer, offset, body, 0, size);
            return body;
        }

        private void Load(int requestCount, int bodySize, bool publishToKafka)
        {
            if (publishToKafka)
            {
                if (SettingsProvider.GetSettings().MergeMessages)
                {
                    var body = GetRandomBody(bodySize * requestCount);
                    kafkaProducer.Produce(TopicName, Guid.NewGuid(), body);
                }
                else
                {
                    for (var i = 0; i < requestCount; i++)
                    {
                        var body = GetRandomBody(bodySize);
                        random.NextBytes(body);
                        kafkaProducer.Produce(TopicName, Guid.NewGuid(), body);
                    }
                }
            }
        }

        [HttpGet]
        public void Error()
        {
            throw new Exception();
        }
    }
}