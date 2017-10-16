using System.Collections.Generic;
using System.Linq;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public class UniformEventGroupGenerator : IEventGroupGenerator
    {
        private readonly string _routingKey;
        private readonly IAirlockEventGenerator _eventGenerator;
        public const int GroupSize = 100;

        public UniformEventGroupGenerator(string routingKey, IAirlockEventGenerator eventGenerator)
        {
            _routingKey = routingKey;
            _eventGenerator = eventGenerator;
        }

        public IEnumerable<EventGroup> Generate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var records = _eventGenerator.Generate().Select(EventRecordHelper.WithBody).Take(GroupSize).ToList();
                yield return new EventGroup
                {
                    RoutingKey = _routingKey,
                    EventRecords = records
                };
            }
        }
    }
}