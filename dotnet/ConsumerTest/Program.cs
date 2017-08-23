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
                Console.WriteLine(DiffTimestampManager.GetReport());
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
            DiffTimestampManager.SetTimestamp(value.Timestamp);
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
        private static DateTime Now;
        private static long timestamp;
        private static int counter;
        private static object lockObject = new object();

        public static void SetTimestamp(long timestampInMilliseconds)
        {
            lock (lockObject)
            {
                Now = DateTime.Now;
                timestamp = timestampInMilliseconds;
                counter++;
            }
        }

        public static string GetReport()
        {
            return $"now: {Now}, now milliseconds:{(Now - new DateTime(1970, 01, 01)).TotalMilliseconds}, timestamp: {timestamp}, count: {counter}";
        }
    }
}
