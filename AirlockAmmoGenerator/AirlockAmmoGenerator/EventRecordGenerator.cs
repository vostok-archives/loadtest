using System;
using System.Collections.Generic;
using System.Linq;

namespace AirlockAmmoGenerator
{
    public class EventRecordHelper
    {
        public static EventRecord WithBody(byte[] body)
            => new EventRecord {Timestamp = DateTimeOffset.Now, Body = body};
    }
}