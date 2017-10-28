using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public interface IEventGeneratorRegistry
    {
        void Add(EventType eventType, IEventGenerator generator);
        IEventGenerator Get(EventType eventType);
    }
}