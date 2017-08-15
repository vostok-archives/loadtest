using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KafkaLoadService.Core
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole((s, l) => l >= LogLevel.Error);
            app.UseMvc(ConfigurateRoutes);
        }

        private static void ConfigurateRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.AddRoute<PingController>("ping", c => c.Ping());
            routeBuilder.AddRoute<PingController>("hello", c => c.PingHelloWorld());
            routeBuilder.AddRoute<PingController>("noop", c => c.PingNoop());
            routeBuilder.AddRoute<PingController>("error", c => c.Error());
            routeBuilder.AddRoute<KafkaLoadController>("kload/timer", c => c.LoadWithTimerAsync(0, 0));
            routeBuilder.AddRoute<KafkaLoadController>("kload", c => c.LoadAsync(0, 0));
            routeBuilder.AddRoute<KafkaLoadController>("kload10", c => c.Load10Async());
            routeBuilder.AddRoute<KafkaLoadController>("kload100", c => c.Load100Async());
            routeBuilder.AddRoute<KafkaLoadController>("kload1000", c => c.Load1000Async());
            routeBuilder.AddRoute<KafkaLoadController>("gen", c => c.GenerateAsync());
        }
    }
}