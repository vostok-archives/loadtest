using System;
using System.Collections.Generic;
using System.Linq;

namespace AirlockAmmoGenerator.Generation
{
    public class CompositeAmmoGenerator : IAmmoGenerator
    {
        private readonly IDictionary<AmmoType, IAmmoGenerator> _generatorRegistry;
        private readonly AmmoType[] _ammoTypes;

        public CompositeAmmoGenerator(IDictionary<AmmoType, IAmmoGenerator> generatorRegistry, params AmmoType[] ammoTypes)
        {
            _generatorRegistry = generatorRegistry;
            _ammoTypes = ammoTypes;
        }

        public IEnumerable<Ammo> Generate(int count)
        {
            int remainingCount = count;
            int step = Math.Max(count / _ammoTypes.Length, 1);
            
            var result = new List<Ammo>(count);
            foreach (var ammoType in _ammoTypes)
            {
                result.AddRange(_generatorRegistry[ammoType].Generate(step));
                remainingCount -= step;
            }

            if (remainingCount > 0)
                result.AddRange(Enumerable.First(_generatorRegistry).Value.Generate(remainingCount));

            Shuffle(result);
            return result;
        }

        private void Shuffle(List<Ammo> result)
        {
            var random = new Random();
            for (int pos = 0; pos < result.Count; pos++)
            {
                var newPos = random.Next(pos, result.Count);
                var tmp = result[pos];
                result[pos] = result[newPos];
                result[newPos] = tmp;
            }
        }
    }
}