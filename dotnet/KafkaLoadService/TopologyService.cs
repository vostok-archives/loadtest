using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaService
{
    public static class TopologyService
    {
        private static Dictionary<string, HashSet<Uri>> map = new Dictionary<string, HashSet<Uri>>();

        public static void Add(string topologyName, string url)
        {
            Add(topologyName, new Uri(url));
        }

        public static void Add(string topologyName, Uri url)
        {
            if (!map.ContainsKey(topologyName))
            {
                map[topologyName] = new HashSet<Uri>();
            }

            map[topologyName].Add(url);
        }

        public static Uri[] GetTopology(string topologyName)
        {
            return map[topologyName].ToArray();
        }
    }
}