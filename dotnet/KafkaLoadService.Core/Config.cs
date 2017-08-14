using System.Collections.Generic;

namespace KafkaLoadService.Core
{
    public class Config
    {
        public Settings Settings { get; set; }
        public Dictionary<string, string[]> Topology { get; set; }
    }
}