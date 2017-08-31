using Microsoft.AspNetCore.Mvc;

namespace ConsumerTest
{
    public class MetricsReportController : ControllerBase
    {
        [HttpGet]
        public string GetLastThroghput()
        {
            return $"{MetricsReporter.LastThroughput}";
        }

        [HttpGet]
        public string GetLastThroghputMb()
        {
            var lastThroghputMb = (double)MetricsReporter.LastThroughputBytes / 1000 / 1000;
            return $"{lastThroghputMb:0.00}";
        }

        [HttpGet]
        public string GetMeanTravelTimeMs()
        {
            return $"{MetricsReporter.LastMeanTravelTimeMs:0.00}";
        }
    }
}