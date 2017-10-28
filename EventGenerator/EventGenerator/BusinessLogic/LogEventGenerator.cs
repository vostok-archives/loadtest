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
            for (int i = 0; i < count; i++)
                _log.Info("Generate info log event");
        }
    }
}