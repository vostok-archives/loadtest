using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlockAmmoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new Options
            {
                AmmoTypes = new[] {AmmoType.Logs},
                Count = 3,
                Host = "localhost",
                Port = 8888,
                ApiKey = "*",
                Output = "output.txt"
            };
            // todo: parse options
            var registry = new Dictionary<AmmoType, IAmmoGenerator>
            {
                {
                    AmmoType.Logs,
                    new AirlockAmmoGenerator(options.Host, options.Port, options.ApiKey, new LogAirlockEventGenerator())
                }
            };
            // todo: fill registry
            var generator = new CompositeAmmoGenerator(registry, options.AmmoTypes);
            var ammo = generator.Generate(options.Count);
            var writer = new FileAmmoWriter(options.Output);
            await writer.WriteAsync(ammo);
        }
    }
}
