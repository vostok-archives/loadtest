using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AirlockAmmoGenerator
{
    public class FileAmmoWriter : IAmmoWriter
    {
        private readonly string _path;

        public FileAmmoWriter(string path)
        {
            _path = path;
        }

        public async Task WriteAsync(IEnumerable<Ammo> ammos)
        {
            using (var file = File.OpenWrite(_path))
            {
                foreach (var ammo in ammos)
                    await WriteAmmo(file, ammo);
            }
        }

        private async Task WriteAmmo(FileStream file, Ammo ammo)
        {
            var bytes = AmmoToBytes(ammo);
            var ammoLength = Encoding.ASCII.GetBytes($"{bytes.Length}\r\n");
            await file.WriteAsync(ammoLength, 0, ammoLength.Length);
            await file.WriteAsync(bytes, 0, bytes.Length);
        }

        private byte[] AmmoToBytes(Ammo ammo)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.ASCII, 1024, true))
                {
                    writer.Write($"POST {ammo.Target.PathAndQuery} HTTP/1.0\r\n");
                    foreach (var header in ammo.Headers)
                        writer.Write($"{header.Key}: {header.Value}\r\n");
                    writer.Write("\r\n");
                }
                stream.Write(ammo.Body, 0, ammo.Body.Length);
                return stream.ToArray();
            }
        }
    }
}