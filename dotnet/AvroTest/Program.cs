using System;
using System.IO;
using System.Linq;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Schema;

namespace AvroTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var schemaString = "{\"type\": \"record\", " +
                               "\"name\": \"kevent\"," +
                               "\"fields\": [" +
                               "{\"name\": \"timestamp\", \"type\": \"long\"}," +
                               "{\"name\": \"payload\", \"type\": \"bytes\"}" +
                               "]}";
            var avroRecord = new AvroRecord(TypeSchema.Create(schemaString));
            avroRecord["timestamp"] = DateTime.UtcNow.Ticks;
            avroRecord["payload"] = Enumerable.Range(0, 10).Select(x => (byte)x).ToArray();
            var totalMilliseconds = DateTime.Now.Subtract(new DateTime(1970, 01, 01)).TotalMilliseconds;
            var dateTime = new DateTime(1970, 01, 01).Add(TimeSpan.FromMilliseconds(1504007964797L));
            var avroSerializer = AvroSerializer.CreateGeneric(schemaString);
            //byte[] bytes;
            //using (var memoryStream = new MemoryStream())
            //{
            //    avroSerializer.Serialize(memoryStream, avroRecord);
            //    bytes = memoryStream.ToArray();
            //    var base64String = Convert.ToBase64String(bytes);
            //}
            try
            {
                var bytes = Convert.FromBase64String("AAAAACn6oeLdxVcUUy+TAh26uvgGwg==");
                using (var memoryStream = new MemoryStream(bytes))
                {
                    var deserialize = avroSerializer.Deserialize(memoryStream);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
