using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace KafkaClient
{
    public class KafkaConsumer<T> : IDisposable
    {
        private readonly Consumer<Null, T> consumer;

        public KafkaConsumer(KafkaSetting kafkaSetting, string topic, IDeserializer<T> deserializer, IObserver<T> observer)
        {
            var settings = kafkaSetting.ToDictionary();
            consumer = new Consumer<Null, T>(settings, new NullDeserializer(), deserializer);
            consumer.OnMessage += (s, e) => observer.OnNext(e.Value);
            consumer.OnError += (s, e) => observer.OnError(new Exception(e.Reason));
            consumer.OnConsumeError += (s, e) => observer.OnError(new Exception(e.Error.Reason));
            consumer.OnPartitionsAssigned += (s, e) =>
            {
                consumer.Assign(e);
            };

            consumer.Subscribe(topic);
            new Task(() =>
            {
                while (true)
                {
                    consumer.Poll(100);
                }
            }).Start();
        }

        public void Dispose()
        {
            consumer?.Dispose();
        }
    }
}