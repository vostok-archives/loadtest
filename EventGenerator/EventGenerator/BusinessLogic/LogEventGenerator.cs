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

        public void Generate(int count)
        {
            _log.Info("Hello, World!");
        }
    }
}