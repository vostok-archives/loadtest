using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KafkaClient;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaProducerService
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverKestrel = new ServerKestrel();
            serverKestrel.Start(8888);
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }

    public static class KafkaProducerProvider
    {
        private static readonly KafkaSetting kafkaSetting;

        static KafkaProducerProvider()
        {
            kafkaSetting = new KafkaSetting()
                .SetBootstrapServers(new Uri("http://localhost:9092"))
                .SetAcks(1)
                .SetRetries(0).SetLinger(TimeSpan.FromMilliseconds(20))
                .Set("socket.blocking.max.ms", 25)
                .Set("batch.num.messages", 64 * 1000)
                .Set("message.max.bytes", 20 * 1000 * 1000)
                .Set("queue.buffering.max.messages", 10000000)
                .Set("queue.buffering.max.kbytes", 2097151)
                .SetClientId("client-id")
                .SetGroupId("test-group");
        }

        public static KafkaProducer Get(Action<byte[]> receiveMessageAction = null)
        {
            return new KafkaProducer(kafkaSetting, receiveMessageAction);
        }
    }

    internal class ServerKestrel
    {
        static ServerKestrel()
        {
            data = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
        }

        public ServerKestrel()
        {
            var options = new KestrelServerOptions
            {
                AddServerHeader = false,
            };
            var optionsWrapper = new OptionsWrapper<KestrelServerOptions>(options);
            var loggerFactory = new LoggerFactory();
            var lifeTime = new ApplicationLifetime(loggerFactory.CreateLogger<ApplicationLifetime>());
            server = new KestrelServer(optionsWrapper, new LibuvTransportFactory(new OptionsManager<LibuvTransportOptions>(
                new OptionsFactory<LibuvTransportOptions>(
                    new List<IConfigureOptions<LibuvTransportOptions>>(), new List<IPostConfigureOptions<LibuvTransportOptions>>())), lifeTime, loggerFactory), loggerFactory);
        }

        public void Start(int port)
        {
            server.Features.Get<IServerAddressesFeature>().Addresses.Add($"http://+:{port}");
            server.StartAsync(new App(), new CancellationToken()).Wait();
        }

        private readonly KestrelServer server;
        private static readonly byte[] data;

        private struct Context
        {
            public Context(IFeatureCollection features)
            {
                Request = features.Get<IHttpRequestFeature>();
                Response = features.Get<IHttpResponseFeature>();
            }

            public IHttpRequestFeature Request { get; }
            public IHttpResponseFeature Response { get; }
        }

        private class App : IHttpApplication<Context>
        {
            private int processedCount;
            private int sentCount;
            private readonly KafkaProducer kafkaProducer;

            public App()
            {
                kafkaProducer = KafkaProducerProvider.Get(x =>
                {
                    Interlocked.Increment(ref processedCount);
                });
            }
            public Context CreateContext(IFeatureCollection contextFeatures)
            {
                return new Context(contextFeatures);
            }

            private static readonly byte[] body = Enumerable.Range(0, 10)
                .Select(i => new Random().Next(256))
                .Select(@int => (byte)@int)
                .ToArray();

            public async Task ProcessRequestAsync(Context context)
            {
                try
                {
                    Interlocked.Increment(ref sentCount);
                    context.Response.StatusCode = 200;
                    var buffer = Encoding.UTF8.GetBytes("ok");
                    context.Response.Headers["Content-Length"] = buffer.Length.ToString();
                    for (var i = 0; i < 1000; i++)
                    {
                        kafkaProducer.Produce("topic", Guid.NewGuid(), body);
                    }
                    if (sentCount % 10000 == 0)
                        Console.WriteLine($"sentCount={sentCount}, processedCount={processedCount}");
                    await context.Response.Body.WriteAsync(buffer , 0, buffer.Length);
                }
                catch (Exception error)
                {
                    Console.Out.WriteLine(error);
                }
            }

            public void DisposeContext(Context context, Exception exception)
            {
            }
        }
    }
}
