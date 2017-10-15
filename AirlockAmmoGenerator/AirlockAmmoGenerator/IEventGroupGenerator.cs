using System;
using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public interface IEventGroupGenerator
    {
        IEnumerable<EventGroup> Generate(int count);
    }
}