using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace AirlockAmmoGenerator
{
    public class Ammo
    {
        public byte[] Body { get; set; }

        public Uri Target { get; set; }

        public Dictionary<string, string> Headers { get; set; } 
    }
}