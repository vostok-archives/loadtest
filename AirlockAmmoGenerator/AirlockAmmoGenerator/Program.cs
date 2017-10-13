using System;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Logging.Airlock;

namespace AirlockAmmoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new Options();
            // todo: parse options
            var registry = default(IAmmoGeneratorRegistry);
            // todo: fill registry
            var generator = new CompositeAmmoGenerator(registry, options.AmmoTypes);
            var ammo = generator.Generate().Take(options.Count);
            var writer = default(IAmmoWriter);
            // todo: implement writer
            await writer.WriteAsync(ammo);

            var s = new LogEventDataSerializer();
            using (var sink = new SimpleAirlockSink())
            {
                s.Serialize(new LogEventData(), sink);
                Console.WriteLine(sink.ToArray().Length);
            }
        }
    }
}
