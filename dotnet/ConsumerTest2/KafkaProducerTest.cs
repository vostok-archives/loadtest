using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaClient;

namespace ConsumerTest2
{
    public class KafkaProducerTest
    {
        private const int stepMilliseconds = 500;
        private static int requestCount;
        private static int successCount;
        private static int errorCount;

        private static void OnMessageDelivered(Message message)
        {
            if (message.Error.HasError)
                Interlocked.Increment(ref errorCount);
            else
                Interlocked.Increment(ref successCount);
        }

        public static double Run(Dictionary<string, int> parameters)
        {
            requestCount = 0;
            successCount = 0;
            errorCount = 0;

            var kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri(Program.KafkaUri))
                .SetAcks(1)
                .SetRetries(0)
                //.Set("auto.commit.interval.ms", 28120)
                //.Set("session.timeout.ms", 41904)
                //.Set("message.max.bytes", 8416000)
                //.Set("message.copy.max.bytes", 920000)
                //.Set("receive.message.max.bytes", 92000000)
                //.Set("max.in.flight.requests.per.connection", 128000)
                //.Set("queue.buffering.max.messages", 9001000)
                //.Set("queue.buffering.max.kbytes", 1887535)
                //.Set("queue.buffering.max.ms", 20)
                //.Set("batch.num.messages", 500500)
                .SetClientId("client-id")
                .SetGroupId("test-group");
            foreach (var parameter in parameters.Where(x => !x.Key.StartsWith("_")))
            {
                kafkaSetting.Set(parameter.Key, parameter.Value);
            }

            double avgRps;
            try
            {
                using (var kafkaProducer = new KafkaProducer(kafkaSetting, OnMessageDelivered))
                {
                    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8888") };

                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;
                    var tasks = new List<Task>();
                    avgRps = 0;
                    var watcherTask = new Task(() =>
                    {
                        var counter = 0;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var prevSuccess = successCount;
                            Thread.Sleep(stepMilliseconds);
                            var newSuccess = successCount;
                            var rps = (double)(newSuccess - prevSuccess) / stepMilliseconds * 1000;
                            if (avgRps > 0 || rps > 0)
                            {
                                counter++;
                                avgRps = (double)successCount / counter / stepMilliseconds * 1000;
                            }
                            Program.Log($"tasks= {tasks.Count}, success = {successCount}, error = {errorCount}, perSecond={rps}, avg={avgRps}");
                        }
                    }, cancellationToken, TaskCreationOptions.LongRunning);
                    watcherTask.Start();

                    for (var i = 0; i < 1; i++)
                    {
                        for (var j = 0; j < parameters["_tasks"]; j++)
                        {
                            var task = new Task(() =>
                            {
                                SendingLoop(kafkaProducer, httpClient, cancellationToken);
                            }, cancellationToken);
                            task.Start();
                            tasks.Add(task);
                        }
                    }
                    Thread.Sleep(60000);
                    cancellationTokenSource.Cancel();
                    Task.WaitAll(tasks.ToArray());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
            Program.Log($"success = {successCount}, all = {requestCount}");
            return avgRps;
        }

        private static readonly byte[] body = Enumerable.Range(0, 10)
            .Select(i => new Random().Next(256))
            .Select(@int => (byte)@int)
            .ToArray();
        private static void SendingLoop(KafkaProducer producer, HttpClient httpClient, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Interlocked.Increment(ref requestCount);
                try
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        producer.Produce(Program.Topic, Guid.NewGuid(), body);
                    }
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref errorCount);
                }
            }
        }
    }
}
