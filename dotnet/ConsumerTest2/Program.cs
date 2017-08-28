using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            KafkaConsumerTest.Run(new Dictionary<string, int>
            {
                ["auto.commit.interval.ms"] = 1000,
                ["session.timeout.ms"] = 6000,
                ["fetch.message.max.bytes"] = 52428800,
                ["fetch.wait.max.ms"] = 500
            });
        }
    }

}
