using System;
using System.Collections.Generic;

namespace AirlockAmmoGenerator.Generation
{
    public class Ammo
    {
        public byte[] Body { get; set; }

        public Uri Target { get; set; }

        public Dictionary<string, string> Headers { get; set; } 
    }
}