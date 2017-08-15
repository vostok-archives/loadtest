using System;
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
                .SetLinger(TimeSpan.FromMilliseconds(20))
                .SetBatchSize(64*1000)
                .SetBufferingSize(256*1000)
                .SetCompression(CompressionCodes.none)
                .SetMetadataRequestTimeout(TimeSpan.FromMilliseconds(25))
                .SetMaxBlocking(TimeSpan.FromMilliseconds(25))
                .SetMaxInFlightRequestsPerConnection(500)
                .SetClientId("client-id");
            kafkaProducer = new KafkaProducer(kafkaSetting);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}