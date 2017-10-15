using System.Collections.Generic;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public interface IEventGroupGenerator
    {
        IEnumerable<EventGroup> Generate(int count);
    }
}