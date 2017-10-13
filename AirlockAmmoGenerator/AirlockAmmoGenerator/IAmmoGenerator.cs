using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public interface IAmmoGenerator
    {
        IEnumerable<Ammo> Generate();
    }
}