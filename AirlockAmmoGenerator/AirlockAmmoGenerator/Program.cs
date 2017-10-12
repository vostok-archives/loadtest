using System;
using Vostok.Logging.Airlock;

namespace AirlockAmmoGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new LogEventDataSerializer();
            using (var sink = new SimpleAirlockSink())
            {
                s.Serialize(new LogEventData(), sink);
                Console.WriteLine(sink.ToArray().Length);
            }
        }
    }
}
