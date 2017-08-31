using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsumerTest
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
            routeBuilder.AddRoute<MetricsReportController>("th", c => c.GetLastThroghput());
            routeBuilder.AddRoute<MetricsReportController>("thmb", c => c.GetLastThroghputMb());
            routeBuilder.AddRoute<MetricsReportController>("mtt", c => c.GetMeanTravelTimeMs());
        }
    }
}