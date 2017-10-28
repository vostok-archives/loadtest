using EventGenerator.BusinessLogic;
using EventGenerator.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vostok.Airlock;
using Vostok.Logging;
using Vostok.Logging.Serilog;

namespace EventGenerator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var serviceProvider = services.BuildServiceProvider();
            var airlockClient = serviceProvider.GetService<IAirlockClient>();
            var service = Configuration.GetValue<string>("service");
            var project = Configuration.GetValue<string>("project");
            var environment = Configuration.GetValue<string>("environment");
            var routingKeyPrefix = RoutingKey.Create(project, environment, service, RoutingKey.LogsSuffix);

            var airlockLogger = new LoggerConfiguration()
                .Enrich.WithProperty("Service", service)
                .WriteTo.Airlock(airlockClient, routingKeyPrefix)
                .CreateLogger();
            var log = new SerilogLog(airlockLogger).WithFlowContext();

            var registry = new EventGeneratorRegistry();
            registry.Add(EventType.Logs, new LogEventGenerator(log));
            registry.Add(EventType.Trace, new TraceEventGenerator());
            services.AddSingleton<IEventGenerationManager>(new EventGenerationManager(registry));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
