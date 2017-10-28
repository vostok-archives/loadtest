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

        public bool Send(EventType argsEventType, int argsCount)
        {
            var generator = _generatorRegistry.Get(argsEventType);
            if (generator != null)
            {
                generator.Generate(argsCount);
                return true;
            }
            return false;
        }
    }
}