using Microsoft.AspNetCore.Mvc;
using TestBase.Model;
using TestBase.Services.Topic;

namespace TestMongoMessagingWeb1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly MessagingService _messagingService;

        public LogController(
            MessagingService messagingService
            )
        {
            _messagingService = messagingService;
        }

        [HttpGet(Name = "GetLog")]
        public async Task<List<string>> GetLogs()
        {
            return Log.Messages;
        }

        [HttpPost(Name = "PublishMessage")]
        public async Task PublishMessage([FromBody] string message)
        {
            await _messagingService.PublishTextAsync(message);
        }
    }
}