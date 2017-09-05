﻿using Microsoft.AspNetCore.Mvc;

namespace KafkaLoadService.Core
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
            var lastThroghputMb = (double)MetricsReporter.LastThroughputBytes / 1024 / 1024;
            return $"{lastThroghputMb:0.00}";
        }
    }
}