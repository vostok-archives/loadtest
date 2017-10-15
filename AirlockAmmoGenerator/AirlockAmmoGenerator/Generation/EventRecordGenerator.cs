using System;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public class EventRecordHelper
    {
        public static EventRecord WithBody(byte[] body)
            => new EventRecord {Timestamp = DateTimeOffset.Now, Body = body};
    }
}