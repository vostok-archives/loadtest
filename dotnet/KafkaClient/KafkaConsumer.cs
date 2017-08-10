using System;
using Confluent.Kafka;

namespace KafkaClient
{
    public class KafkaConsumer : IDisposable
    {
        private readonly Consumer consumer;

        public KafkaConsumer(KafkaSetting kafkaSetting, string topic, int partition, EventHandler<Message> process)
        {
            var settings = kafkaSetting.ToDictionary();
            consumer = new Consumer(settings);
            consumer.OnMessage += process;

            consumer.Assign(new[] {new TopicPartition(topic, partition)});
            consumer.Poll(100);
        }

        public void Pool(int millisecondsTimeout)
        {
            consumer.Poll(millisecondsTimeout);
        }

        public void Dispose()
        {
            consumer?.Dispose();
        }
    }
}