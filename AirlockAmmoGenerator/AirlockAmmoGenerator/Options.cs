using AirlockAmmoGenerator.Generation;

namespace AirlockAmmoGenerator
{
    public class Options
    {
        private AmmoType[] _ammoTypes = new AmmoType[0];

        public int Count { get; set; }

        public AmmoType[] AmmoTypes
        {
            get => _ammoTypes;
            set => _ammoTypes = value ?? new AmmoType[0];
        }

        public string Output { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string ApiKey { get; set; }
    }
}