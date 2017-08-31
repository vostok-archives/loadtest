using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace KafkaClient
{
    public class SimpleDesiralizer : IDeserializer<byte[]>
    {
        public byte[] Deserialize(byte[] data)
        {
            return data;
        }
    }

    public class KafkaConsumer<T> : IDisposable
    {
        private readonly Consumer<byte[], T> consumer;
        private readonly CancellationTokenSource cancellationTokenSource;

        public KafkaConsumer(KafkaSetting kafkaSetting, string topic, IDeserializer<T> deserializer, IObserver<Message<byte[], T>> observer)
        {
            var settings = kafkaSetting.ToDictionary();
            consumer = new Consumer<byte[], T>(settings, new SimpleDesiralizer(), deserializer);
            consumer.OnMessage += (s, e) => observer.OnNext(e);
            consumer.OnError += (s, e) => observer.OnError(new Exception(e.Reason));
            consumer.OnConsumeError += (s, e) => observer.OnError(new Exception(e.Error.Reason));
            consumer.OnPartitionsAssigned += (s, e) =>
            {
                Console.WriteLine($"Assigned partitions: [{string.Join(", ", e)}], member id: {consumer.MemberId}");
                consumer.Assign(e.Select(x => new TopicPartitionOffset(x, Offset.Stored)));
            };
            consumer.OnPartitionsRevoked += (_, e) =>
            {
                Console.WriteLine($"Revoked partitions: [{string.Join(", ", e)}]");
                consumer.Unassign();
            };

            //consumer.Assign(new [] { new TopicPartitionOffset(topic, 0, 0) });
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            consumer.Subscribe(topic);
            new Task(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //Message<byte[], T> msg;
                    //if (consumer.Consume(out msg, 1000))
                    //{
                    //    observer.OnNext(msg.Value);
                    //}
                    consumer.Poll(100);
                }
            }).Start();
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            consumer?.Dispose();
        }
    }
}