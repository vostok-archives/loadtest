using KafkaClient;

namespace KafkaLoadService.Core
{
    public static class KafkaProducerProvider
    {
        private static readonly KafkaProducer kafkaProducer;

        static KafkaProducerProvider()
        {

            var topology = TopologyService.GetTopology("Kafka");

            var producerSetting = new KafkaSetting();
            producerSetting.AddBootstrapServers(topology);
            producerSetting.AddClientId("producer_1");

            kafkaProducer = new KafkaProducer(producerSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}