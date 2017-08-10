using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using KafkaClient;

namespace KafkaService
{
    public class KafkaLoadController : ApiController
    {
        private const string TopicName = "topic-kload-dot-net";
        private readonly KafkaProducer kafkaProducer;

        public KafkaLoadController()
        {
            kafkaProducer = GetProducer();
        }

        [HttpGet]
        public void LoadWithTimer(int requestCount, int bodySize)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Load(requestCount, bodySize);
            stopwatch.Stop();
            Console.WriteLine(
                $"{nameof(requestCount)}: {requestCount}, {nameof(bodySize)}: {bodySize}, ElapsedMilliseconds: {stopwatch.ElapsedMilliseconds}");
        }


        [HttpGet]
        public void Load10() => Load(100, 10);
        [HttpGet]
        public void Load100() => Load(100, 100);
        [HttpGet]
        public void Load1000() => Load(100, 1000);
        [HttpGet]
        public void Generate() => Load(100, 1000, false);

        [HttpGet]
        public void Load(int requestCount, int bodySize) => Load(requestCount, bodySize, true);

        private void Load(int requestCount, int bodySize, bool publishToKafka)
        {
            var random = new Random();
            for (var i = 0; i < requestCount; i++)
            {
                var body = GenerateBody(random, bodySize);
                if (publishToKafka)
                {
                    kafkaProducer.Produce(TopicName, Guid.NewGuid(), body);
                }
            }
        }

        private static byte[] GenerateBody(Random random, int bodySize)
        {
            return Enumerable.Range(0, bodySize)
                .Select(i => random.Next(256))
                .Select(@int => (byte) @int)
                .ToArray();
        }

        private static KafkaProducer GetProducer()
        {
            var topology = TopologyService.GetTopology("Kafka");

            var producerSetting = new KafkaSetting();
            producerSetting.AddBootstrapServers(topology);
            producerSetting.AddClientId("producer_1");

            return new KafkaProducer(producerSetting);
        }

        [HttpGet]
        public void Error()
        {
            throw new Exception();
        }
    }
}