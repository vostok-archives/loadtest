using System;
using System.Collections.Generic;
using AirlockAmmoGenerator.Gate;
using AirlockAmmoGenerator.Generation;
using Vostok.Logging;
using Vostok.Logging.Airlock;

namespace AirlockAmmoGenerator.Special
{
    public class LogAirlockEventGenerator : IAirlockEventGenerator
    {
        private readonly LogEventDataSerializer _eventSerializer;

        public LogAirlockEventGenerator()
        {
            _eventSerializer = new LogEventDataSerializer();
        }

        IEnumerable<byte[]> IAirlockEventGenerator.Generate()
        {
            while (true)
            {
                using (var sink = new SimpleAirlockSink())
                {
                    _eventSerializer.Serialize(new LogEventData
                    {
                        Message = "Hello, World!",
                        Timestamp = DateTimeOffset.Now,
                        Level = LogLevel.Info,
                        Properties = new Dictionary<string, string>()
                    }, sink);
                    yield return sink.ToArray();
                }
            }
        }
    }
}