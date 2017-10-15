using System.Collections.Generic;

namespace AirlockAmmoGenerator.Generation
{
    public interface IAirlockEventGenerator
    {
        IEnumerable<byte[]> Generate();
    }
}