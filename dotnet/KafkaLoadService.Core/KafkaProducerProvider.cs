using KafkaClient;

namespace KafkaLoadService.Core
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaProducer kafkaProducer;

        static KafkaProducerProvider()
        {
            var topology = TopologyService.GetTopology("Kafka");
            var settings = SettingsProvider.GetSettings();
            var kafkaSetting = new KafkaSetting(settings.DisableKafkaReports)
                .SetBootstrapServers(topology)
                .SetAcks(1)
                .SetRetries(0)
                .Set("queue.buffering.max.ms", 20)
                .Set("socket.blocking.max.ms", 25)
                .Set("batch.num.messages", 64*1000)
                .Set("message.max.bytes", 20*1000*1000)
                .Set("queue.buffering.max.messages", 10000000)
                .Set("queue.buffering.max.kbytes", 2097151)
                .Set("message.copy.max.bytes", 10000000)
                .Set("socket.send.buffer.bytes", 10000000)
                .Set("max.in.flight.requests.per.connection", 500)
                .Set("compression.codec", "none")
                .Set("socket.keepalive.enable", true)
                .Set("socket.timeout.ms", 20)
                .SetClientId("client-id");
            kafkaProducer = new KafkaProducer(kafkaSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}