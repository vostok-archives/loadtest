using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConsumerTest2
{
    public class ParameterInfo {
        public string Name { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }

    public class Program
    {
        public static readonly string KafkaUri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "http://localhost:9092" : "http://icat-test01:9092";
        public const string Topic = "dot-net";

        private static readonly object lockObject = new object();
        public static void Log(string msg)
        {
            msg = $"{DateTime.Now.ToLongTimeString()} {msg}";
            Console.WriteLine(msg);
            //Logger.Info(msg);
            lock (lockObject)
            {
                File.AppendAllText("log.txt", msg + "\n");
            }
        }

        static void Main(string[] args)
        {
            Log("kafka: " + KafkaUri);
            //Util.ConfigureLog4Net();
            //Logger = LogManager.GetLogger(typeof(Program));
            //.Set("auto.commit.interval.ms", parameters["auto.commit.interval.ms"]) //1000
            //    .Set("session.timeout.ms", parameters["session.timeout.ms"]) //60000
            //    .Set("fetch.message.max.bytes", parameters["fetch.message.max.bytes"]) //52428800
            //    .Set("fetch.wait.max.ms", parameters["fetch.wait.max.ms"]) //500
            KafkaProducerTest.Run(new Dictionary<string, int>());
            KafkaConsumerTest.Run(new Dictionary<string, int>());
            //ParamsOptimization();
        }

        private static void ParamsOptimization(Func<Dictionary<string,int>,double> func)
        {
            var parameterInfos = new[]
            {
                //new ParameterInfo()
                //{
                //    Name = "fetch.message.max.bytes",
                //    MinValue = 1000000,
                //    MaxValue = 100000000
                //}, 

                new ParameterInfo
                {
                    Name = "auto.commit.interval.ms",
                    MinValue = 25000,
                    MaxValue = 32800
                },
                new ParameterInfo
                {
                    Name = "session.timeout.ms",
                    MinValue = 35600,
                    MaxValue = 43480
                },
                new ParameterInfo
                {
                    Name = "message.max.bytes",
                    MinValue = 8020000,
                    MaxValue = 10000000
                },
                new ParameterInfo
                {
                    Name = "message.copy.max.bytes",
                    MinValue = 600000,
                    MaxValue = 1000000
                },
                new ParameterInfo
                {
                    Name = "receive.message.max.bytes",
                    MinValue = 80000000,
                    MaxValue = 100000000
                },
                new ParameterInfo
                {
                    Name = "max.in.flight.requests.per.connection",
                    MinValue = 80000,
                    MaxValue = 160000
                },
                new ParameterInfo
                {
                    Name = "queue.buffering.max.messages",
                    MinValue = 8002000,
                    MaxValue = 10000000
                },
                new ParameterInfo
                {
                    Name = "queue.buffering.max.kbytes",
                    MinValue = 1677920,
                    MaxValue = 2097151
                },
                new ParameterInfo
                {
                    Name = "queue.buffering.max.ms",
                    MinValue = 5000,
                    MaxValue = 10000
                },
                new ParameterInfo
                {
                    Name = "batch.num.messages",
                    MinValue = 1000,
                    MaxValue = 1000000
                },
            };
            for (var i = 0; i < 10; i++)
            {
                const int pointCount = 5;
                foreach (var parameterInfo in parameterInfos)
                {
                    var currentParams = parameterInfos.ToDictionary(x => x.Name, x => (x.MaxValue + x.MinValue) / 2);
                    double bestResult = 0;
                    var bestPoint = 0;
                    var diff = (double) (parameterInfo.MaxValue - parameterInfo.MinValue) / pointCount;
                    for (var j = 0; j <= pointCount; j++)
                    {
                        currentParams[parameterInfo.Name] = (int) Math.Round(parameterInfo.MinValue + j * diff);
                        Log("Current optimized param: " + parameterInfo.Name + " = " + currentParams[parameterInfo.Name]);
                        Log("test params:\n  " + String.Join("\n  ", currentParams.Select(x => $"{x.Key} => {x.Value}")));

                        //var result = KafkaQueueFiller.Run(currentParams);
                        var result = func(currentParams);
                        if (result > bestResult)
                        {
                            bestResult = result;
                            bestPoint = j;
                        }
                    }
                    parameterInfo.MaxValue = bestPoint == pointCount
                        ? parameterInfo.MaxValue
                        : (int) (parameterInfo.MinValue + (bestPoint + 1) * diff);
                    parameterInfo.MinValue = bestPoint == 0
                        ? parameterInfo.MinValue
                        : (int) (parameterInfo.MinValue + (bestPoint - 1) * diff);
                    Log("ParameterInfos:\n  " + String.Join("\n  ",
                            parameterInfos.Select(x => $"{x.Name} => {x.MinValue} .. {x.MaxValue}")));
                }
            }
        }
    }

}
