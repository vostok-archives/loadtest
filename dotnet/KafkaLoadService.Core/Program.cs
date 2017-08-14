using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.DotNet.PlatformAbstractions;

namespace KafkaLoadService.Core
{
    class Program
    {
        static void Main()
        {
            var configPath = Path.Combine(ApplicationEnvironment.ApplicationBasePath, "config.json");
            SettingsProvider.FillFromFile(configPath);
            var settings = SettingsProvider.GetSettings();
            var baseAddress = $"http://*:{settings.ServicePort}/";

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