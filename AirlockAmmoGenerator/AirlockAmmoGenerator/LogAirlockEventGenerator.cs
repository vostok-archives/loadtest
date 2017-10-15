using System;
using System.Collections.Generic;
using Vostok.Logging.Airlock;

namespace AirlockAmmoGenerator
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
                        Timestamp = DateTimeOffset.Now
                    }, sink);
                    yield return sink.ToArray();
                }
            }
        }
    }
}