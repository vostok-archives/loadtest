using System.IO;
using Vostok.Commons.Binary;

namespace AirlockAmmoGenerator.Gate
{
    public class AirlockMessageSerializer
    {
        public byte[] Serialize(AirlockMessage message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new SimpleAirlockBinaryWriter(stream))
            {
                writer.Write((short) 1);
                writer.WriteCollection(message.EventGroups, SerializeEventGroup);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private void SerializeEventGroup(IBinaryWriter writer, EventGroup eventGroup)
        {
            writer.Write(eventGroup.RoutingKey);
            writer.WriteCollection(eventGroup.EventRecords, SerializeEventRecord);
        }

        private void SerializeEventRecord(IBinaryWriter writer, EventRecord record)
        {
            writer.Write(record.Timestamp.UtcTicks);
            writer.Write(record.Body);
        }
    }
}