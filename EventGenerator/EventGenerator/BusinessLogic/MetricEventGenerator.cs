using System;
using System.Threading.Tasks;
using Vostok.Metrics;
using Vostok.Metrics.Meters;

namespace EventGenerator.BusinessLogic
{
    public class MetricEventGenerator : IEventGenerator
    {
        private readonly ICounter _counter;

        public MetricEventGenerator(IMetricScope scope)
        {
            _counter = scope.Counter(TimeSpan.FromMilliseconds(100), "generated");
        }

        public Task Generate(int count)
        {
            for (var i = 0; i < count; i++)
                _counter.Add();
            return Task.FromResult(0);
        }
    }
}