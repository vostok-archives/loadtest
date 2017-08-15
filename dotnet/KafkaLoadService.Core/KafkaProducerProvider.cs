using KafkaClient;

namespace KafkaLoadService.Core
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaProducer kafkaProducer;

        static KafkaProducerProvider()
        {

            var topology = TopologyService.GetTopology("Kafka");

            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(topology)
                .SetAcks(1)
                .SetRetries(0)
                .Set("queue.buffering.max.ms", 20)
                .Set("socket.blocking.max.ms", 25)
                .SetClientId("client-id");
            kafkaProducer = new KafkaProducer(kafkaSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}