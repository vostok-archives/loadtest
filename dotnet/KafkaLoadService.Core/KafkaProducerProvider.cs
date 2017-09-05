using System;
using System.IO;
using System.Linq;
using Confluent.Kafka;
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

        private static void OnMessageSent(Message message)
        {
            if (!message.Error.HasError)
                MetricsReporter.Produced(1, message.Value.Length);
            else
            {
                MetricsReporter.Error(1);
                Console.WriteLine(message.Error.Reason);
            }
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
                .Set("auto.commit.interval.ms", 1400)
                .Set("session.timeout.ms", 8400)
                .Set("message.max.bytes", 1000000)
                .Set("message.copy.max.bytes", 604000)
                //.Set("receive.message.max.bytes", 92000000)
                .Set("max.in.flight.requests.per.connection", 560000)
                .Set("queue.buffering.max.messages", 9200800)
                .Set("queue.buffering.max.kbytes", 839460)
                .Set("queue.buffering.max.ms", 500)
                .Set("batch.num.messages", 1000000)
                .SetClientId("client-id");
            return new KafkaProducer(kafkaSetting, OnMessageSent);
        }

        public static KafkaProducer Get()
        {
            return kafkaProducer;
        }
    }
}