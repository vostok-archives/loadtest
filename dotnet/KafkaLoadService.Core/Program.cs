using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace KafkaLoadService.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new Exception("Bad starting format. Template: \"KafkaService.exe {selfPort} {kafkaUri}\".");
            }

            
            var selfPort = args[0];
            var kafkaTopology = args[1];
            TopologyService.Add("Kafka", kafkaTopology);
            var baseAddress = $"http://*:{selfPort}/";

            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(baseAddress)
                .Build()
                .Run();
        }
    }
}