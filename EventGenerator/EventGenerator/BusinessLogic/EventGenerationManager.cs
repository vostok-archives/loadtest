using System.Threading.Tasks;
using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public class EventGenerationManager : IEventGenerationManager
    {
        private readonly IEventGeneratorRegistry _generatorRegistry;

        public EventGenerationManager(IEventGeneratorRegistry generatorRegistry)
        {
            _generatorRegistry = generatorRegistry;
        }

        public async Task<bool> SendAsync(EventType argsEventType, int argsCount)
        {
            var generator = _generatorRegistry.Get(argsEventType);
            if (generator != null)
            {
                await generator.Generate(argsCount);
                return true;
            }
            return false;
        }
    }
}