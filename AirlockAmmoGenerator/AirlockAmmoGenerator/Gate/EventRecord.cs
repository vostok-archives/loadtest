using System;

namespace AirlockAmmoGenerator.Gate
{
    public class EventRecord
    {
        public DateTimeOffset Timestamp { get; set; }
        public byte[] Body { get; set; }
    }
}