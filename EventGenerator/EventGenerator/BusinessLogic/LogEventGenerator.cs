using System.Threading.Tasks;
using Vostok.Logging;

namespace EventGenerator.BusinessLogic
{
    public class LogEventGenerator : IEventGenerator
    {
        private readonly ILog _log;

        public LogEventGenerator(ILog log)
        {
            _log = log;
        }

        public Task Generate(int count)
        {
            _log.Info("Hello, World!");
            return Task.FromResult(true);
        }
    }
}