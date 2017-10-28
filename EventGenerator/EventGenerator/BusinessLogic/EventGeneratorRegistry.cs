using System.Collections.Concurrent;
using EventGenerator.Models;

namespace EventGenerator.BusinessLogic
{
    public class EventGeneratorRegistry : IEventGeneratorRegistry
    {
        private readonly ConcurrentDictionary<EventType, IEventGenerator> _registry;

        public EventGeneratorRegistry() => _registry = new ConcurrentDictionary<EventType, IEventGenerator>();

        public void Add(EventType eventType, IEventGenerator generator) => _registry[eventType] = generator;

        public IEventGenerator Get(EventType eventType) =>
            _registry.TryGetValue(eventType, out var result) ? result : null;
    }
}