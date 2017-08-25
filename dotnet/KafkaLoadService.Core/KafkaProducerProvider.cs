using System;
using System.Linq;
using KafkaClient;

namespace KafkaLoadService.Core
{
    public static class KafkaProducerProvider
    {
        private static KafkaProducer[] kafkaProducers;

        static KafkaProducerProvider()
        {
            kafkaProducers = Enumerable.Range(0, 10)
                .Select(x => CreateKafkaProducer())
                .ToArray();
        }

        private static KafkaProducer CreateKafkaProducer()
        {
            var topology = TopologyService.GetTopology("Kafka");
            var settings = SettingsProvider.GetSettings();
            var kafkaSetting = new KafkaSetting(settings.DisableKafkaReports)
                .SetBootstrapServers(topology)
                .SetAcks(1)
                .SetRetries(0)
                //.Set("queue.buffering.max.ms", 20)
                .Set("socket.blocking.max.ms", 50)
                .Set("batch.num.messages", 64 * 1000)
                .Set("message.max.bytes", 20 * 1000 * 1000)
                .Set("queue.buffering.max.messages", 10000000)
                .Set("queue.buffering.max.kbytes", 2097151)
                .Set("message.copy.max.bytes", 10000000)
                .Set("socket.send.buffer.bytes", 10000000)
                .Set("max.in.flight.requests.per.connection", 500)
                .Set("compression.codec", "none")
                .Set("socket.keepalive.enable", true)
                .Set("socket.timeout.ms", 20)
                .SetClientId("client-id");
            return new KafkaProducer(kafkaSetting);
        }

        public static KafkaProducer Get()
        {
            var random = new Random();
            return kafkaProducers[random.Next(10)];
        }
    }
}