using System;
using System.Collections.Generic;
using System.Globalization;
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
        public const string Topic = "dot-net1";

        private static readonly object lockObject = new object();
        public static void Log(string msg)
        {
            msg = $"{DateTime.Now:u} {msg}";
            Console.WriteLine(msg);
            //Logger.Info(msg);
            lock (lockObject)
            {
                File.AppendAllText("log.txt", msg + "\n");
            }
        }
        public static void LogStat(IEnumerable<string> columns)
        {
            var msg = $"{DateTime.Now:u} {string.Join(";", columns)}";
            Console.WriteLine(msg);
            //Logger.Info(msg);
            lock (lockObject)
            {
                File.AppendAllText("stat.txt", msg + "\n");
            }
        }

        static void Main(string[] args)
        {
            Log("kafka: " + KafkaUri);
            Log("topic: " + Topic);
            //Util.ConfigureLog4Net();
            //Logger = LogManager.GetLogger(typeof(Program));
            //.Set("auto.commit.interval.ms", parameters["auto.commit.interval.ms"]) //1000
            //    .Set("session.timeout.ms", parameters["session.timeout.ms"]) //60000
            //    .Set("fetch.message.max.bytes", parameters["fetch.message.max.bytes"]) //52428800
            //    .Set("fetch.wait.max.ms", parameters["fetch.wait.max.ms"]) //500
            KafkaProducerTest.Run(new Dictionary<string, int>());
            //KafkaConsumerTest.Run(new Dictionary<string, int>());
            ParamsOptimization(KafkaConsumerTest.Run);
        }

        private static void ParamsOptimization(Func<Dictionary<string,int>,double> func)
        {
            var parameterInfos = new[]
            {
                ////// CONSUMER //////////////////////
                //new ParameterInfo
                //{
                //    Name = "queued.min.messages",
                //    MinValue = 10000,
                //    MaxValue = 10000000
                //},
                //new ParameterInfo
                //{
                //    Name = "queued.max.messages.kbytes",
                //    MinValue = 10000,
                //    MaxValue = 10000000
                //},
                new ParameterInfo
                {
                    Name = "fetch.message.max.bytes",
                    MinValue = 10,
                    MaxValue = 10000
                },
                //new ParameterInfo
                //{
                //    Name = "fetch.wait.max.ms",
                //    MinValue = 100,
                //    MaxValue = 10000
                //},
                //new ParameterInfo
                //{
                //    Name = "receive.message.max.bytes",
                //    MinValue = 100000,
                //    MaxValue = 100000000
                //},
                //new ParameterInfo
                //{
                //    Name = "max.in.flight.requests.per.connection",
                //    MinValue = 1000,
                //    MaxValue = 800000
                //},
                //new ParameterInfo
                //{
                //    Name = "session.timeout.ms",
                //    MinValue = 1600,
                //    MaxValue = 43480
                //},
                //new ParameterInfo
                //{
                //    Name = "batch.num.messages",
                //    MinValue = 1000,
                //    MaxValue = 1000000
                //},

                /*
                ////// PRODUCER ///////////////////
                //new ParameterInfo
                //{
                //    Name = "auto.commit.interval.ms",
                //    MinValue = 1000,
                //    MaxValue = 62800
                //},
                //new ParameterInfo
                //{
                //    Name = "session.timeout.ms",
                //    MinValue = 1600,
                //    MaxValue = 43480
                //},
                new ParameterInfo
                {
                    Name = "message.max.bytes",
                    MinValue = 10000,
                    MaxValue = 10000000
                },
                //new ParameterInfo
                //{
                //    Name = "message.copy.max.bytes",
                //    MinValue = 10000,
                //    MaxValue = 1000000
                //},
                new ParameterInfo
                {
                    Name = "max.in.flight.requests.per.connection",
                    MinValue = 1000,
                    MaxValue = 800000
                },
                //new ParameterInfo
                //{
                //    Name = "queue.buffering.max.messages",
                //    MinValue = 8002000,
                //    MaxValue = 10000000
                //},
                //new ParameterInfo
                //{
                //    Name = "queue.buffering.max.kbytes",
                //    MinValue = 1000,
                //    MaxValue = 2097151
                //},
                //new ParameterInfo
                //{
                //    Name = "queue.buffering.max.ms",
                //    MinValue = 100,
                //    MaxValue = 10000
                //},
                //new ParameterInfo
                //{
                //    Name = "batch.num.messages",
                //    MinValue = 1000,
                //    MaxValue = 1000000
                //},
                */
            };
            LogStat(parameterInfos.Select(x => x.Name).Concat(new [] { "value" }));
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
                        Log("test params:\n  " + string.Join("\n  ", currentParams.Select(x => $"{x.Key} => {x.Value}")));

                        //var result = KafkaQueueFiller.Run(currentParams);
                        var result = func(currentParams);
                        if (result > bestResult)
                        {
                            bestResult = result;
                            bestPoint = j;
                        }
                        LogStat(parameterInfos.Select(x => currentParams[x.Name].ToString()).Concat(new[] { result.ToString(CultureInfo.InvariantCulture) }));
                    }
                    parameterInfo.MaxValue = bestPoint == pointCount
                        ? parameterInfo.MaxValue
                        : (int) (parameterInfo.MinValue + (bestPoint + 1) * diff);
                    parameterInfo.MinValue = bestPoint == 0
                        ? parameterInfo.MinValue
                        : (int) (parameterInfo.MinValue + (bestPoint - 1) * diff);
                    Log("ParameterInfos:\n  " + string.Join("\n  ", parameterInfos.Select(x => $"{x.Name} => {x.MinValue} .. {x.MaxValue}")));
                }
            }
        }
    }

}
