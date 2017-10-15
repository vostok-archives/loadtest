using System.Collections.Generic;
using System.Linq;
using AirlockAmmoGenerator.Gate;

namespace AirlockAmmoGenerator.Generation
{
    public class SingleGroupMessageGenerator : IMessageGenerator
    {
        private readonly IEventGroupGenerator _eventGroupGenerator;

        public SingleGroupMessageGenerator(IEventGroupGenerator eventGroupGenerator)
        {
            _eventGroupGenerator = eventGroupGenerator;
        }

        public IEnumerable<AirlockMessage> Generate(int count) =>
            from g in _eventGroupGenerator.Generate(count)
            select new AirlockMessage {EventGroups = new List<EventGroup> {g}};
    }
}