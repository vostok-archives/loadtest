using System;
using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public class AirlockMessage
    {
        public List<EventGroup> EventGroups { get; set; }
    }

    public class EventGroup
    {
        public string RoutingKey { get; set; }
        public List<EventRecord> EventRecords {get; set; }
    }

    public class EventRecord
    {
        public DateTimeOffset Timestamp { get; set; }
        public byte[] Body { get; set; }
    }
}