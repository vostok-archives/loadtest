using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaClient
{
    ///     librdkafka configuration parameters (refer to https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md).
    public class KafkaSetting
    {
        public bool DisableDeliveryReports { get; set; }

        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();

        public KafkaSetting SetBootstrapServers(params Uri[] servers)
        {
            var setting = string.Join(",", servers.Select(s => $"{s.Host}:{s.Port}"));
            return Set("bootstrap.servers", setting);
        }

        public KafkaSetting SetAcks(int count)
            => Set("acks", count);

        public KafkaSetting SetRetries(int count)
            => Set("retries", count);

        public KafkaSetting SetLinger(TimeSpan timeout)
            => Set("queue.buffering.max.ms", timeout.TotalMilliseconds);

        public KafkaSetting SetMessageTimeout(TimeSpan timeout)
            => Set("message.timeout.ms", (int)timeout.TotalMilliseconds);

        public KafkaSetting SetTopicMetadataRefreshInterval(TimeSpan timeout)
            => Set("topic.metadata.refresh.interval.ms", (int)timeout.TotalMilliseconds);

        public KafkaSetting SetBatchSize(int size)
            => Set("queue.buffering.max.messages", size);

        public KafkaSetting SetSocketSendBuffer(int size)
            => Set("socket.send.buffer.bytes", size);

        public KafkaSetting SetBufferingSize(int sizeInKBytes)
            => Set("queue.buffering.max.kbytes", sizeInKBytes);

        public KafkaSetting SetRequestTimeout(TimeSpan timeout)
            => Set("request.timeout.ms", timeout.Milliseconds);

        public KafkaSetting SetCompression(CompressionCodes code)
            => Set("compression.codec", code.ToString());

        public KafkaSetting SetMetadataRequestTimeout(TimeSpan timeout)
            => Set("metadata.request.timeout.ms", timeout.TotalMilliseconds);

        public KafkaSetting SetMaxBlocking(TimeSpan timeout)
            => Set("socket.blocking.max.ms", timeout.TotalMilliseconds);

        public KafkaSetting SetMaxInFlightRequestsPerConnection(int count)
            => Set("max.in.flight.requests.per.connection", count);

        public KafkaSetting SetGroupId(string groupId)
            => Set("group.id", groupId);

        public KafkaSetting SetClientId(string clientId)
            => Set("client.id", clientId);

        public KafkaSetting Set(string key, object setting)
        {
            settings[key] = setting;

            return this;
        }

        public IEnumerable<KeyValuePair<string, object>> ToDictionary()
        {
            return settings;
        }
    }
}