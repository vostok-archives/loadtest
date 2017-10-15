using System.Collections.Generic;

namespace AirlockAmmoGenerator.Generation
{
    public interface IAmmoGenerator
    {
        IEnumerable<Ammo> Generate(int count);
    }
}