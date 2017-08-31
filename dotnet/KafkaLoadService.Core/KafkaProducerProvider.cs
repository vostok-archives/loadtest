using System;
using System.IO;
using System.Linq;
using KafkaClient;

namespace KafkaLoadService.Core
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaProducer kafkaProducer;

        static KafkaProducerProvider()
        {
            kafkaProducer = CreateKafkaProducer();
        }

        private static void OnMessageSent(byte[] bytes)
        {
            
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
            return new KafkaProducer(kafkaSetting, OnMessageSent);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}