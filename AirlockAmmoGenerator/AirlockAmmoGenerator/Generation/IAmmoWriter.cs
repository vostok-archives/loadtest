using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlockAmmoGenerator.Generation
{
    public interface IAmmoWriter
    {
        Task WriteAsync(IEnumerable<Ammo> ammos);
    }
}