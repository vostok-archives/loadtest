using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsumerTest2;
using KafkaClient;

namespace ConsumerTest
{
    public class KafkaConsumerTest
    {
        public static int MessageCount;

        private class MessageObserver : IObserver<byte[]>
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
                //.SetCompression(CompressionCodes.none)
                .Set("auto.offset.reset", "latest")
                .Set("auto.commit.interval.ms", parameters["auto.commit.interval.ms"]) //1000
                .Set("session.timeout.ms", parameters["session.timeout.ms"]) //60000
                .Set("fetch.message.max.bytes", parameters["fetch.message.max.bytes"]) //52428800
                .Set("fetch.wait.max.ms", parameters["fetch.wait.max.ms"]) //500
                .SetClientId("client-id")
                .SetGroupId("test-group");

            KafkaQueueFiller.Run();

            var consumers = Enumerable.Range(1, 1)
                .Select(x => new KafkaConsumer<byte[]>(kafkaSetting, KafkaQueueFiller.Topic, new SimpleDesiralizer(),
                    new MessageObserver()))
                .ToArray();
            var counter = 0;
            const int stepMilliseconds = 1000;
            double avgRps;
            while (true)
            {
                var prevCount = MessageCount;
                Thread.Sleep(TimeSpan.FromMilliseconds(stepMilliseconds));
                counter++;
                var newCount = MessageCount;
                var rps = (double)(newCount - prevCount) / stepMilliseconds * 1000;
                avgRps = (double)newCount / counter / stepMilliseconds * 1000;
                //Console.WriteLine(DiffTimestampManager.GetReport());
                Program.Log($"MessageCount={newCount}, perSecond={rps}, avg={avgRps}");
                if (Math.Abs(rps) < 1 && newCount > 0)
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