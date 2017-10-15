using System.Collections.Generic;
using System.Linq;

namespace AirlockAmmoGenerator
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