using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace KafkaClient
{
    public class KafkaProducer : IDisposable
    {
        private readonly Producer producer;
        private readonly bool disableDeliveryReports;

        public KafkaProducer(KafkaSetting kafkaSetting)
        {
            var settings = kafkaSetting.ToDictionary();
            disableDeliveryReports = kafkaSetting.DisableDeliveryReports;
            producer = new Producer(settings, false, disableDeliveryReports);
        }

        public void Produce(string topic, Guid key, byte[] value)
        {
            ProduceAsync(topic, key, value).GetAwaiter().GetResult();
        }

        public async Task ProduceAsync(string topic, Guid key, byte[] value)
        {
            var task = producer.ProduceAsync(topic, key.ToByteArray(), value);
            if (!disableDeliveryReports)
            {
                var message = await task.ConfigureAwait(false);

                if (message.Error.HasError)
                {
                    throw new Exception(message.Error.Reason);
                }
            }
        }

        public void Dispose()
        {
            producer.Dispose();
        }
    }
}