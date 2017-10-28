using System.Threading.Tasks;
using EventGenerator.Models;
using Vostok.Logging;

namespace EventGenerator.BusinessLogic
{
    public class EventGenerationManager : IEventGenerationManager
    {
        private readonly ILog _log;

        public EventGenerationManager(ILog log)
        {
            _log = log;
        }

        public Task<bool> SendAsync(EventType argsEventType, int argsCount)
        {
            _log.Info("Hello, World!");
            return Task.FromResult(true);
        }
    }
}