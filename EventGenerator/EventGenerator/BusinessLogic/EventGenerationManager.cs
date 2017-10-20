using System.Threading.Tasks;
using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public class EventGenerationManager : IEventGenerationManager
    {
        public Task<bool> SendAsync(EventType argsEventType, int argsCount) => Task.FromResult(true);
    }
}