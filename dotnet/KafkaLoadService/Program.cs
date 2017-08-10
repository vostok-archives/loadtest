using System;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.Owin.Hosting;
using Owin;

namespace KafkaService
{
    static class Program
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
            var baseAddress = $"http://+:{selfPort}/";

            using (WebApp.Start(baseAddress, Configurate))
            {
                Console.WriteLine("Server started");
                Console.ReadLine();
            }
        }

        private static void Configurate(IAppBuilder builder)
        {
            var configuration = new HttpConfiguration();
            ConfigurateRoutes(configuration.Routes);
            configuration.Filters.Add(new ImpExceptionFilterAttribute());
            builder.UseWebApi(configuration);

        }

        private static void ConfigurateRoutes(HttpRouteCollection routes)
        {
            routes.AddRoute<PingController>("ping", c => c.Ping());
            routes.AddRoute<PingController>("hello", c => c.PingHelloWorld());
            routes.AddRoute<PingController>("noop", c => c.PingNoop());
            routes.AddRoute<KafkaLoadController>("kload/timer", c => c.LoadWithTimer(0, 0));
            routes.AddRoute<KafkaLoadController>("kload", c => c.Load(0, 0));
            routes.AddRoute<KafkaLoadController>("kload10", c => c.Load10());
            routes.AddRoute<KafkaLoadController>("kload100", c => c.Load100());
            routes.AddRoute<KafkaLoadController>("kload1000", c => c.Load1000());
            routes.AddRoute<KafkaLoadController>("gen", c => c.Generate());
            routes.AddRoute<PingController>("error", c => c.Error());
        }
    }

    public class ImpExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Console.WriteLine(actionExecutedContext.Exception);
            base.OnException(actionExecutedContext);
        }
    }

}
