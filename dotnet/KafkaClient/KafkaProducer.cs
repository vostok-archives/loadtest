using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace KafkaClient
{
    public class KafkaProducer : IDisposable
    {
        private class DeliveryHandler : IDeliveryHandler
        {
            private readonly Action<Message> messageAction;

            public DeliveryHandler(Action<Message> messageAction)
            {
                this.messageAction = messageAction;
            }
            public void HandleDeliveryReport(Message message)
            {
                messageAction(message);
            }

            public bool MarshalData
            {
                get { return true; }
            }
        }

        private readonly Producer producer;
        private readonly bool disableDeliveryReports;
        private readonly DeliveryHandler deliveryHandler;

        public KafkaProducer(KafkaSetting kafkaSetting, Action<Message> receiveMessageAction = null)
        {
            if (receiveMessageAction != null)
            {
                deliveryHandler = new DeliveryHandler(receiveMessageAction);
            }
            var settings = kafkaSetting.ToDictionary();
            disableDeliveryReports = kafkaSetting.DisableDeliveryReports;
            producer = new Producer(settings, false, disableDeliveryReports);
        }

        public void Produce(string topic, Guid key, byte[] value)
        {
            if (deliveryHandler != null)
                producer.ProduceAsync(topic, key.ToByteArray(), value, deliveryHandler);
            else
                ProduceAsync(topic, key, value).GetAwaiter().GetResult();
        }

        public Task ProduceAsync(string topic, Guid key, byte[] value)
        {
            var task = producer.ProduceAsync(topic, key.ToByteArray(), value);
            if (disableDeliveryReports)
            {
                return Task.CompletedTask;
            }
            return CheckResultAsync(task);
        }

        private async Task CheckResultAsync(Task<Message> task)
        {
            var message = await task.ConfigureAwait(false);

            //Console.WriteLine($"Produced partition:{message.Partition}, offset:{message.Offset}, topic:{message.Topic}");
            if (message.Error.HasError)
            {
                throw new Exception(message.Error.Reason);
            }
            //producer.Flush(10000);
        }

        public void Dispose()
        {
            producer.Flush(10000);
            producer.Dispose();
        }
    }
}