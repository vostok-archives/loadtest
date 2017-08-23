using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using Confluent.Kafka.Serialization;
using KafkaClient;
using Microsoft.Hadoop.Avro;

namespace ConsumerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri("http://icat-test01:9092"))
                .SetGroupId("test-group");

            var kafkaConsumer = new KafkaConsumer<TestKaskaModel>(kafkaSetting, "ktopic-with-ts", new AvroDeserializer<TestKaskaModel>(), new MessageObserver());


            var cancellationToken = new CancellationToken();
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"diff: {DiffTimestampManager.Diff.TotalMilliseconds} ms");
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }

            kafkaConsumer.Dispose();
        }
    }

    public class AvroDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(byte[] data)
        {
            var avroSerializer = AvroSerializer.Create<T>();
            using (var memoryStream = new MemoryStream(data))
            {
                return avroSerializer.Deserialize(memoryStream);
            }
        }
    }

    public class MessageObserver : IObserver<TestKaskaModel>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnNext(TestKaskaModel value)
        {
            var now = DateTime.Now;
            DiffTimestampManager.Diff = now - new DateTime(value.Timestamp);
        }
    }

    [DataContract(Name = "record")]
    public class TestKaskaModel
    {
        [DataMember(Name = "timestamp")]
        public long Timestamp { get; set; }
        [DataMember(Name = "payload")]
        public byte[] Payload { get; set; }
    }

    public static class DiffTimestampManager
    {
        private static TimeSpan diff;
        private static object lockObject = new object();
        public static TimeSpan Diff
        {
            get => diff;
            set
            {
                lock (lockObject)
                {
                    diff = value;
                }
            }
        }
    }
}
