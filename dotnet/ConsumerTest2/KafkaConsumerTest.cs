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
                .SetClientId("client-id")
                .SetGroupId("test-group");
            foreach (var parameter in parameters)
            {
                kafkaSetting.Set(parameter.Key, parameter.Value);
            }

            KafkaQueueFiller.Run(parameters);

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