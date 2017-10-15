using System.Collections.Generic;

namespace AirlockAmmoGenerator.Gate
{
    public class EventGroup
    {
        public string RoutingKey { get; set; }
        public List<EventRecord> EventRecords {get; set; }
    }
}