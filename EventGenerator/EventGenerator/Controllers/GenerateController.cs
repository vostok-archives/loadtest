using System.Threading.Tasks;
using EventGenerator.BusinessLogic;
using EventGenerator.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventGenerator.Controllers
{
    [Route("/[controller]")]
    public class GenerateController : Controller
    {
        private readonly IEventGenerationManager _manager;

        public GenerateController(IEventGenerationManager manager)
        {
            _manager = manager;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] GenerateEventsArgs args)
        {
            if (ModelState.IsValid)
            {
                var ok = await _manager.SendAsync(args.EventType, args.Count);
                return ok ? Ok() : StatusCode(500);
            }
            return BadRequest(ModelState);
        }
    }
}