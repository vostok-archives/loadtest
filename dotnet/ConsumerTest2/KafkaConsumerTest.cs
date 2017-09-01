using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using KafkaClient;

namespace ConsumerTest2
{
    public class KafkaConsumerTest
    {
        public static int MessageCount;

        private class MessageObserver : IObserver<Message<byte[],byte[]>>
        {
            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
                Console.WriteLine(error);
            }

            public void OnNext(Message<byte[], byte[]> value)
            {
                Interlocked.Increment(ref MessageCount);
            }
        }

        public static double Run(Dictionary<string, int> parameters)
        {
            MessageCount = 0;
            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri(Program.KafkaUri))
                .SetAcks(1)
                .SetRetries(0)
                .SetCompression(CompressionCodes.none)
                .Set("fetch.message.max.bytes",2000)
                //.Set("auto.offset.reset", "latest")
                .SetClientId("client-id")
                .SetGroupId("test-group");
            foreach (var parameter in parameters)
            {
                kafkaSetting.Set(parameter.Key, parameter.Value);
            }

            var consumers = Enumerable.Range(1, 1)
                .Select(x => new KafkaConsumer<byte[]>(kafkaSetting, Program.Topic, new SimpleDesiralizer(),new MessageObserver()))
                .ToArray();
            var counter = 0;
            const int stepMilliseconds = 1000;
            double avgRps = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                var prevCount = MessageCount;
                Thread.Sleep(TimeSpan.FromMilliseconds(stepMilliseconds));
                var newCount = MessageCount;
                var rps = (double)(newCount - prevCount) / stepMilliseconds * 1000;
                if (avgRps > 0 || rps > 0)
                {
                    counter++;
                    avgRps = (double)newCount / counter / stepMilliseconds * 1000;
                }
                //Console.WriteLine(DiffTimestampManager.GetReport());
                Program.Log($"MessageCount={newCount}, perSecond={rps}, avg={avgRps}");
                if (Math.Abs(rps) < 1 && newCount > 0 || stopwatch.ElapsedMilliseconds > 60000)
                    break;
            }
            foreach (var consumer in consumers)
            {
                consumer.Dispose();
            }
            return avgRps;
        }
    }
}