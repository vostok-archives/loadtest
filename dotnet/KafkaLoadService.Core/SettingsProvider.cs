using System.IO;
using Newtonsoft.Json;

namespace KafkaLoadService.Core
{
    public static class SettingsProvider
    {
        private static Settings settings;

        public static void FillFromFile(string path)
        {
            var jsonSerializer = new JsonSerializer();
            using (var streamReader = new StreamReader(File.OpenRead(path)))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var config = jsonSerializer.Deserialize<Config>(jsonTextReader);
                    settings = config.Settings;
                    TopologyService.Fill(config.Topology);
                }
            }
        }

        public static Settings GetSettings()
        {
            return settings;
        }
    }
}