using System.Collections.Generic;

namespace AirlockAmmoGenerator
{
    public class AmmoGeneratorRegistry : IAmmoGeneratorRegistry
    {
        private Dictionary<AmmoType, IAmmoGenerator> _generators;

        public AmmoGeneratorRegistry()
        {
            _generators = new Dictionary<AmmoType, IAmmoGenerator>();
        }

        public IAmmoGenerator Get(AmmoType ammoType)
        {
            throw new System.NotImplementedException();
        }

        public IAmmoGenerator Set(AmmoType ammoType, IAmmoGenerator generator)
        {
            throw new System.NotImplementedException();
        }
    }
}