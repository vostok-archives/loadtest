using System;
using System.IO;
using System.Linq;
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
                .Set("fetch.message.max.bytes", 10000000)
                .Set("message.max.bytes", 1000000000)
                .Set("message.copy.max.bytes", 1000000000)
                .Set("receive.message.max.bytes", 1000000000)
                .Set("max.in.flight.requests.per.connection", 1000000)
                .Set("socket.send.buffer.bytes", 100000000)
                .Set("socket.receive.buffer.bytes", 100000000)
                .Set("queued.min.messages", 10000000)
                .Set("fetch.min.bytes", 1)
                .Set("queued.max.messages.kbytes", 1000000000)
                .Set("fetch.wait.max.ms", 10000);

            var kafkaConsumer = new KafkaConsumer<byte[]>(kafkaSetting, "ktopic-with-ts", new DefaultDeserializer(), new CounterObserver<byte[]>());

            var cancellationToken = new CancellationToken();
            while (!cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(StepMilliseconds));

                Console.WriteLine($"count: {MetricsReporter.TotalCount}, throughput: {MetricsReporter.LastThroughput}"
                    + $", throughput MB: {(double) MetricsReporter.LastThroughputBytes / 1000 / 1000:0.00}"
                    + $", mean travel time ms {MetricsReporter.LastMeanTravelTimeMs}");
            }

            kafkaConsumer.Dispose();
        }
    }

    public class AvroDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(byte[] data)
        {
            var avroSerializer = AvroSerializer.Create<T>();
            var buffer = data.Skip(5).ToArray();
            using (var memoryStream = new MemoryStream(buffer))
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

    public class CounterObserver<T> : IObserver<T>
    {
        public void OnCompleted()
        { }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnNext(T value)
        {
            MetricsReporter.Add(1, 0, 0);
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
            MetricsReporter.Add(1, value.Payload.Length, value.Timestamp);
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
}
