using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace AirlockAmmoGenerator
{
    public class Ammo
    {
        public byte[] Body { get; set; }

        public Uri Path { get; set; }

        public string ApiKey { get; set; }

        public HttpRequestHeaders Headers { get; set; } 
    }
}