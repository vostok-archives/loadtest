namespace AirlockAmmoGenerator
{
    public class Options
    {
        private AmmoType[] _ammoTypes = new AmmoType[0];

        public int Count { get; set; }

        //public bool GenerateLogAmmo { get; set; }
        //public bool GenerateMetricsAmmo { get; set; }
        //public bool GenerateMetricsAggregatorAmmo { get; set; }
        //public bool GenerateTracingAmmo { get; set; }
        public AmmoType[] AmmoTypes
        {
            get => _ammoTypes;
            set => _ammoTypes = value ?? new AmmoType[0];
        }

        public string Output { get; set; }
    }
}