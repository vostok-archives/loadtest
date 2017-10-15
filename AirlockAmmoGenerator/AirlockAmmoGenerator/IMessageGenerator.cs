using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public interface IMessageGenerator
    {
        IEnumerable<AirlockMessage> Generate(int count);
    }
}