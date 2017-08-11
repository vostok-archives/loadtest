using Microsoft.AspNetCore.Mvc;

namespace KafkaLoadService.Core
{
    public class PingController : ControllerBase
    {
        [HttpGet]
        public string Ping()
        {
            return "Ok";
        }

        [HttpGet]
        public string PingHelloWorld()
        {
            return "Hello, world!";
        }

        [HttpGet]
        public void PingNoop()
        {
        }

        [HttpGet]
        public void Error()
        {
            throw new System.NotImplementedException();
        }
    }
}