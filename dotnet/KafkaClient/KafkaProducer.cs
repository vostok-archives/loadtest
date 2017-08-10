using System;
using Confluent.Kafka;

namespace KafkaClient
{
    public class KafkaProducer : IDisposable
    {
        private readonly Producer producer;

        public KafkaProducer(KafkaSetting kafkaSetting)
        {
            var settings = kafkaSetting.ToDictionary();
            producer = new Producer(settings, false, true);
        }

        public void Produce(string topic, Guid key, byte[] value)
        {
            producer.ProduceAsync(topic, key.ToByteArray(), value);
        }

        public void Dispose()
        {
            producer.Dispose();
        }
    }
}