using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public interface IAirlockEventGenerator
    {
        IEnumerable<byte[]> Generate();
    }
}