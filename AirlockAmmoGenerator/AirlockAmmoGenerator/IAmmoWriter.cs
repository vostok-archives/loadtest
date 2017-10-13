using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirlockAmmoGenerator
{
    public interface IAmmoWriter
    {
        Task WriteAsync(IEnumerable<Ammo> ammos);
    }
}