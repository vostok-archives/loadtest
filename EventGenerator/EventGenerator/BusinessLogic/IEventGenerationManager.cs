using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public interface IEventGenerationManager
    {
        bool Send(EventType argsEventType, int argsCount);
    }
}