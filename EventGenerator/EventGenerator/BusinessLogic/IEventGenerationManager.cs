using System.Threading.Tasks;
using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public interface IEventGenerationManager
    {
        Task<bool> SendAsync(EventType argsEventType, int argsCount);
    }
}