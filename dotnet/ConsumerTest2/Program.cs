using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Confluent.Kafka.Serialization;
using KafkaClient;
using log4net;
using Microsoft.Hadoop.Avro;

namespace ConsumerTest
{
    class Program
    {
        public static int MessageCount = 0;
        public static ILog Logger;

        public static readonly KafkaSetting KafkaSetting = new KafkaSetting()
            .SetBootstrapServers(new Uri("http://localhost:9092"))
            .SetAcks(1)
            .SetRetries(0)
            .Set("auto.offset.reset", "latest")
            .Set("auto.commit.interval.ms", 1000)
            .Set("session.timeout.ms", 60000)
            .Set("fetch.message.max.bytes", 5242880)
            .Set("fetch.wait.max.ms", 500)
            .Set("socket.blocking.max.ms", 25)
            .Set("batch.num.messages", 64 * 1000)
            .Set("message.max.bytes", 20 * 1000 * 1000)
            .Set("queue.buffering.max.messages", 10000000)
            .Set("queue.buffering.max.kbytes", 2097151)
            .SetClientId("client-id")
            .SetGroupId("test-group");

        public static void Log(string msg)
        {
            //Console.WriteLine($"{DateTime.Now.ToShortTimeString()} {msg}");
            Logger.Info(msg);
        }

        static void Main(string[] args)
        {
            Util.ConfigureLog4Net();
            Logger = LogManager.GetLogger(typeof(Program));
            KafkaQueueFiller.Run();

            var consumers = Enumerable.Range(1, 3)
                .Select(x => new KafkaConsumer<byte[]>(Program.KafkaSetting, "topic", new SimpleDesiralizer(),
                    new MessageObserver()))
                .ToArray();
            var counter = 0;
            const int stepMilliseconds = 1000;
            while (true)
            {
                var prevCount = MessageCount;
                Thread.Sleep(TimeSpan.FromMilliseconds(stepMilliseconds));
                counter++;
                var newCount = MessageCount;
                var rps = (double)(newCount - prevCount) / stepMilliseconds * 1000;
                var avgRps = (double)newCount / counter / stepMilliseconds * 1000;
                //Console.WriteLine(DiffTimestampManager.GetReport());
                Log($"MessageCount={newCount}, perSecond={rps}, avg={avgRps}");
                if (Math.Abs(rps) < 1 && newCount > 0)
                    break;
            }
            foreach (var consumer in consumers)
            {
                consumer.Dispose();
            }
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

    public class MessageObserver : IObserver<byte[]>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnNext(byte[] value)
        {
            Interlocked.Increment(ref Program.MessageCount);
            //Console.WriteLine("Got!");
            var now = DateTime.Now;
            //DiffTimestampManager.SetTimestamp(value.Timestamp);
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
