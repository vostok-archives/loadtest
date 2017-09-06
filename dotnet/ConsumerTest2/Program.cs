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
        public double[] Values = new double[Program.PointCount+1];
    }

    public class Program
    {
        public const int PointCount = 5;
        public static readonly string KafkaUri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Environment.MachineName.StartsWith("icat", StringComparison.InvariantCultureIgnoreCase) ? "http://localhost:9092" : "http://icat-test01:9092";
        public static string Topic = "dot-net1";

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
            //Util.ConfigureLog4Net();
            //Logger = LogManager.GetLogger(typeof(Program));
            //.Set("auto.commit.interval.ms", parameters["auto.commit.interval.ms"]) //1000
            //    .Set("session.timeout.ms", parameters["session.timeout.ms"]) //60000
            //    .Set("fetch.message.max.bytes", parameters["fetch.message.max.bytes"]) //52428800
            //    .Set("fetch.wait.max.ms", parameters["fetch.wait.max.ms"]) //500
            //KafkaProducerTest.Run(new Dictionary<string, int>());
            //KafkaConsumerTest.Run(new Dictionary<string, int>());
            ParamsOptimization(x =>
            {
                Topic = "dot-net" + Guid.NewGuid();
                Log("topic: " + Topic);
                return KafkaProducerTest.Run(x);
                //return KafkaConsumerTest.Run(x);
            });
        }

        private static void ParamsOptimization(Func<Dictionary<string,int>,double> func)
        {
            var parameterInfos = new[]
            {
                ////// CONSUMER //////////////////////
/*                new ParameterInfo
                {
                    Name = "queued.min.messages",
                    MinValue = 10000000, //10000
                    MaxValue = 10000000
                },
                new ParameterInfo
                {
                    Name = "queued.max.messages.kbytes",
                    MinValue = 100000, //10000
                    MaxValue = 100000000
                },
                new ParameterInfo
                {
                    Name = "fetch.message.max.bytes",
                    MinValue = 10,
                    MaxValue = 100 //10000000
                },
                new ParameterInfo
                {
                    Name = "fetch.wait.max.ms",
                    MinValue = 100,
                    MaxValue = 10000
                },
                new ParameterInfo
                {
                    Name = "receive.message.max.bytes",
                    MinValue = 5000000, //100000
                    MaxValue = 100000000
                },
                new ParameterInfo
                {
                    Name = "max.in.flight.requests.per.connection",
                    MinValue = 200000, //1000
                    MaxValue = 800000
                },
                new ParameterInfo
                {
                    Name = "session.timeout.ms",
                    MinValue = 3000, //1600
                    MaxValue = 12000 //43480
                },
                //new ParameterInfo
                //{
                //    Name = "batch.num.messages",
                //    MinValue = 1000,
                //    MaxValue = 1000000
                //},*/

                
                ////// PRODUCER ///////////////////
                new ParameterInfo
                {
                    Name = "_tasks",
                    MinValue = 2,
                    MaxValue = 2
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
                    MinValue = 100000, //1000
                    MaxValue = 2097151
                },
                new ParameterInfo
                {
                    Name = "queue.buffering.max.ms",
                    MinValue = 10,
                    MaxValue = 1000 //10000
                },

                //new ParameterInfo
                //{
                //    Name = "auto.commit.interval.ms",
                //    MinValue = 1000, //1000
                //    MaxValue = 5000 //10000
                //},
                new ParameterInfo
                {
                    Name = "message.max.bytes",
                    MinValue = 10000, //10000
                    MaxValue = 1000000
                },
                //new ParameterInfo
                //{
                //    Name = "message.copy.max.bytes",
                //    MinValue = 10000,
                //    MaxValue = 1000000
                //},
                new ParameterInfo
                {
                    Name = "batch.num.messages",
                    MinValue = 1000000, //1000
                    MaxValue = 1000000
                },
                new ParameterInfo
                {
                    Name = "max.in.flight.requests.per.connection",
                    MinValue = 800000, //1000
                    MaxValue = 1000000
                },
                //new ParameterInfo
                //{
                //    Name = "session.timeout.ms",
                //    MinValue = 3000, //1600
                //    MaxValue = 12000 //43480
                //},
            };
            LogStat(parameterInfos.Select(x => x.Name).Concat(new [] { "RPS" }));
            for (var i = 0; i < 10; i++)
            {
                foreach (var parameterInfo in parameterInfos.Where(x => x.MaxValue - x.MinValue > 3))
                {
                    var currentParams = parameterInfos.ToDictionary(x => x.Name, x => (x.MaxValue + x.MinValue) / 2);
                    double bestResult = 0;
                    var bestPoint = 0;
                    var diff = (double) (parameterInfo.MaxValue - parameterInfo.MinValue) / PointCount;
                    var results = new double[PointCount+1];
                    for (var j = 0; j <= PointCount; j++)
                    {
                        currentParams[parameterInfo.Name] = (int)Math.Round(parameterInfo.MinValue + j * diff);
                        Log("Current optimized param: " + parameterInfo.Name + " = " + currentParams[parameterInfo.Name]);
                        Log("test params:\n  " + string.Join("\n  ", currentParams.Select(x => $"{x.Key} => {x.Value}")));
                        double result;
                        //result = parameterInfo.Values[j] > 0 ? parameterInfo.Values[j] : func(currentParams);
                        result = func(currentParams);
                        results[j] = result;
                        if (result > bestResult)
                        {
                            bestResult = result;
                            bestPoint = j;
                        }
                        parameterInfo.Values[j] = 0;
                        LogStat(parameterInfos.Select(x => currentParams[x.Name].ToString()).Concat(new[] { ((long)result).ToString(CultureInfo.InvariantCulture) }));
                    }
                    var firstPoint = bestPoint == 0 ? 0 : bestPoint - 1;
                    var lastPoint = bestPoint == PointCount ? PointCount : bestPoint + 1;
                    parameterInfo.MaxValue = (int)(parameterInfo.MinValue + lastPoint * diff);
                    parameterInfo.MinValue = (int)(parameterInfo.MinValue + firstPoint * diff);
                    parameterInfo.Values[0] = results[firstPoint];
                    parameterInfo.Values[PointCount] = results[lastPoint];
                    Log("ParameterInfos:\n  " + string.Join("\n  ", parameterInfos.Select(x => $"{x.Name} => {x.MinValue} .. {x.MaxValue}")));
                }
            }
        }
    }

}
