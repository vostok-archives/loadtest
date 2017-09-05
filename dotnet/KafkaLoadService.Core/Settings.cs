namespace KafkaLoadService.Core
{
    public class Settings
    {
        public string ServicePort { get; set; }
        public bool DisableKafkaReports { get; set; }
        public bool MergeMessages { get; set; }
    }
}