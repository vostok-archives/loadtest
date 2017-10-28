using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging;
using Vostok.Logging.Serilog;

namespace EventGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", false, true);
                })
                .UseUrls("http://*:5000")
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAirlock()
                .ConfigureVostokMetrics()
                .ConfigureVostokTracing()
                .UseStartup<Startup>()
                .Build();

        private static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder builder)
        {
            var loggingSection = context.Configuration.GetSection("logging");
            var rollingFileSection = loggingSection.GetSection("rollingFile");
            var rollingFilePathFormat = rollingFileSection.GetValue<string>("pathFormat");
            var service = context.Configuration.GetValue<string>("service");
            var template = "{Timestamp:HH:mm:ss.fff} {Level} {Message:l} {Exception}{NewLine}{Properties}{NewLine}";

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Service", service)
                .WriteTo.Async(x => x.RollingFile(rollingFilePathFormat, outputTemplate: template))
                .CreateLogger();
            var log = new SerilogLog(Log.Logger).WithFlowContext();

            builder.AddVostok(log);
            builder.Services.AddSingleton(log);
        }
    }
}
