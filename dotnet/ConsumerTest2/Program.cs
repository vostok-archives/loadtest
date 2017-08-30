using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ConsumerTest2;
using KafkaClient;

namespace ConsumerTest
{

    public class ParameterInfo {
        public string Name { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
    }

    class Program
    {
        public const string KafkaUri = "http://localhost:9092";

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
            //Util.ConfigureLog4Net();
            //Logger = LogManager.GetLogger(typeof(Program));
            //.Set("auto.commit.interval.ms", parameters["auto.commit.interval.ms"]) //1000
            //    .Set("session.timeout.ms", parameters["session.timeout.ms"]) //60000
            //    .Set("fetch.message.max.bytes", parameters["fetch.message.max.bytes"]) //52428800
            //    .Set("fetch.wait.max.ms", parameters["fetch.wait.max.ms"]) //500
            var parameterInfos = new ParameterInfo[]
            {
                new ParameterInfo
                {
                    Name = "auto.commit.interval.ms",
                    MinValue = 1,
                    MaxValue = 100000
                }, 
                new ParameterInfo
                {
                    Name = "session.timeout.ms",
                    MinValue = 1000,
                    MaxValue = 60000
                }, 
                new ParameterInfo
                {
                    Name = "message.max.bytes",
                    MinValue = 1000,
                    MaxValue = 10000000
                }, 
                new ParameterInfo
                {
                    Name = "message.copy.max.bytes",
                    MinValue = 0,
                    MaxValue = 1000000
                }, 
                new ParameterInfo
                {
                    Name = "receive.message.max.bytes",
                    MinValue = 1000,
                    MaxValue = 100000000
                }, 
                new ParameterInfo
                {
                    Name = "max.in.flight.requests.per.connection",
                    MinValue = 1,
                    MaxValue = 1000000
                }, 
                new ParameterInfo
                {
                    Name = "queue.buffering.max.messages",
                    MinValue = 1,
                    MaxValue = 10000000
                }, 
                new ParameterInfo
                {
                    Name = "queue.buffering.max.kbytes",
                    MinValue = 1,
                    MaxValue = 2097151
                }, 
                new ParameterInfo
                {
                    Name = "queue.buffering.max.ms",
                    MinValue = 0,
                    MaxValue = 10000
                }, 
                new ParameterInfo
                {
                    Name = "batch.num.messages",
                    MinValue = 1,
                    MaxValue = 1000000
                }, 
            };
            for (var i = 0; i < 10; i++)
            {
                const int pointCount = 10;
                foreach (var parameterInfo in parameterInfos)
                {
                    var currentParams = parameterInfos.ToDictionary(x => x.Name, x => (x.MaxValue + x.MinValue) / 2);
                    double bestResult = 0;
                    var bestPoint = 0;
                    var diff = (double)(parameterInfo.MaxValue - parameterInfo.MinValue) / pointCount;
                    for (var j = 0; j <= pointCount; j++)
                    {
                        currentParams[parameterInfo.Name] = (int) Math.Round(parameterInfo.MinValue + j * diff);
                        Log("Current optimized param: " + parameterInfo.Name);
                        Log("test params:\n  " + string.Join("\n  ", currentParams.Select(x => $"{x.Key} => {x.Value}")));

                        var result = KafkaQueueFiller.Run(currentParams);
                        if (result > bestResult)
                        {
                            bestResult = result;
                            bestPoint = j;
                        }
                    }
                    parameterInfo.MinValue = bestPoint == 0 ? parameterInfo.MinValue : (int)(parameterInfo.MinValue + (bestPoint-1) * diff);
                    parameterInfo.MaxValue = bestPoint == pointCount ? parameterInfo.MaxValue : (int)(parameterInfo.MinValue + (bestPoint + 1) * diff);
                }
            }
        }
    }

}
