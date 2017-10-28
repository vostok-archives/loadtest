using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventGenerator.Models
{
    public class GenerateEventsArgs
    {
        [Required]
        public EventType EventType { get; set; }

        [Range(1, 10000)]
        public int Count { get; set; } = 1;
    }
}