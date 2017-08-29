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
        private const int StepMilliseconds = 500;
        static void Main(string[] args)
        {
            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri("http://icat-test01:9092"))
                .SetGroupId("test-group2")
                .Set("auto.offset.reset", "latest")
                .Set("auto.commit.interval.ms", 1000)
                .Set("queued.max.messages.kbytes", 1000000000)
                .Set("queued.min.messages", 10000000)
                .Set("fetch.message.max.bytes", 510000)
                .Set("fetch.wait.max.ms", 500);

            var kafkaConsumer = new KafkaConsumer<byte[]>(kafkaSetting, "ktopic-with-ts", new DefaultDeserializer(), new CounterObserver());

            var cancellationToken = new CancellationToken();
            while (!cancellationToken.IsCancellationRequested)
            {
                var oldvalue = Counter.Value;
                Thread.Sleep(TimeSpan.FromMilliseconds(StepMilliseconds));
                var newValue = Counter.Value;
                Console.WriteLine($"{((double)newValue - oldvalue)/StepMilliseconds*1000}\t{newValue}");
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
                    var result = (AvroRecord) avroSerializer.Deserialize(memoryStream);
                    return new TestKafkaModel
                    {
                        Timestamp = result.GetField<long>("timestamp"),
                        Payload = result.GetField<byte[]>("payload")
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

    public class DefaultDeserializer : IDeserializer<byte[]>
    {
        public byte[] Deserialize(byte[] data)
        {
            return data;
        }
    }

    public class CounterObserver : IObserver<byte[]>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnNext(byte[] value)
        {
            Counter.Inc();
        }
    }

    public static class Counter
    {
        private static int value = 0;
        public static int Value => value;

        public static void Inc()
        {
            Interlocked.Increment(ref value);
        }
    }

    public class TestKafkaModelObserver : IObserver<TestKafkaModel>
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
