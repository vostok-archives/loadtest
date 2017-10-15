using System.Collections.Generic;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public interface IMessageGenerator
    {
        IEnumerable<AirlockMessage> Generate(int count);
    }
}