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
                .SetGroupId("test-group2");

            var kafkaConsumer = new KafkaConsumer<TestKafkaModel>(kafkaSetting, "ktopic-with-ts", new AvroTestKafkaModelDeserializer(), new MessageObserver());


            var cancellationToken = new CancellationToken();
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine(TimestampInfoManager.GetReport());
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

    public class AvroTestKafkaModelDeserializer : IDeserializer<TestKafkaModel>
    {
        public TestKafkaModel Deserialize(byte[] data)
        {
            var schemaString = "{\"type\": \"record\", " +
                                  "\"name\": \"kevent\"," +
                                  "\"fields\": [" +
                                  "{\"name\": \"timestamp\", \"type\": \"long\"}," +
                                  "{\"name\": \"payload\", \"type\": \"bytes\"}" +
                                  "]}";
            var avroSerializer = AvroSerializer.CreateGeneric(schemaString);
            try
            {
                using (var memoryStream = new MemoryStream(data))
                {
                    dynamic result = avroSerializer.Deserialize(memoryStream);
                    Console.WriteLine(result.GetType());
                    return new TestKafkaModel
                    {
                        Timestamp = result["timestamp"],
                        Payload = result["payload"]
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class MessageObserver : IObserver<TestKafkaModel>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnNext(TestKafkaModel value)
        {
            TimestampInfoManager.SetTimestamp(value.Timestamp);
        }
    }

    [DataContract(Name = "kevent")]
    public class TestKafkaModel
    {
        [DataMember(Name = "timestamp")]
        public long Timestamp { get; set; }
        [DataMember(Name = "payload")]
        public byte[] Payload { get; set; }
    }

    public static class TimestampInfoManager
    {
        private static DateTime now;
        private static long timestamp;
        private static int counter;
        private static readonly object lockObject = new object();

        public static void SetTimestamp(long timestampInMilliseconds)
        {
            lock (lockObject)
            {
                now = DateTime.Now;
                timestamp = timestampInMilliseconds;
                counter++;
            }
        }

        public static string GetReport()
        {
            return $"now: {now}, now milliseconds:{(now - new DateTime(1970, 01, 01)).TotalMilliseconds}, timestamp: {timestamp}, count: {counter}";
        }
    }
}
