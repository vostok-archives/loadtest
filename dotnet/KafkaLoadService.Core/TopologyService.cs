using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaLoadService.Core
{
    public static class TopologyService
    {
        private static Dictionary<string, HashSet<Uri>> map = new Dictionary<string, HashSet<Uri>>();

        public static void Fill(Dictionary<string, string[]> topology)
        {
            map = topology.ToDictionary(x => x.Key, x => new HashSet<Uri>(x.Value.Select(uriString => new Uri(uriString))));
        }

        public static Uri[] GetTopology(string topologyName)
        {
            return Enumerable.ToArray(map[topologyName]);
        }
    }
}