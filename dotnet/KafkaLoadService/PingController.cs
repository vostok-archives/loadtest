using System.Web.Http;

namespace KafkaService
{
    public class PingController : ApiController
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