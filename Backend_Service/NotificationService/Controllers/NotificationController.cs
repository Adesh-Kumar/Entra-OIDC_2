using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Controllers
{

 //   [Authorize]
    [ApiController]
    [Route("api/notifications")]

    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(ILogger<NotificationController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called.");
            return Ok(new { Message = "Notification service is working!" });
        }
    }
}