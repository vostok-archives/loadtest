using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaClient
{
    public class KafkaSetting
    {
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();

        public void AddBootstrapServers(params Uri[] servers)
        {
            var setting = string.Join(",", servers.Select(s => $"{s.Host}:{s.Port}"));
            settings["bootstrap.servers"] = setting;
        }

        public void AddClientId(string clientId)
        {
            settings["client.id"] = clientId;
        }

        public void AddTimeout(TimeSpan timeout)
        {
            settings["request.timeout.ms"] = timeout.Milliseconds;
        }

        public void AddGroupId(string groupId)
        {
            settings["group.id"] = groupId;
        }

        public IEnumerable<KeyValuePair<string, object>> ToDictionary()
        {
            return settings;
        }
    }
}