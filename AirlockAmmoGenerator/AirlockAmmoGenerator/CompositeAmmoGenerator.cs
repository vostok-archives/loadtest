using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public class CompositeAmmoGenerator : IAmmoGenerator
    {
        public CompositeAmmoGenerator(IAmmoGeneratorRegistry generatorRegistry, params AmmoType[] ammoTypes)
        {
        }

        public IEnumerable<Ammo> Generate()
        {
            throw new System.NotImplementedException();
        }
    }
}