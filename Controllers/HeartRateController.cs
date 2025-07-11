using Microsoft.AspNetCore.Mvc;
using HikarinHeartRateMonitorService.Models;

namespace HikarinHeartRateMonitorService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeartRateController : ControllerBase
    {
        private readonly ILogger<HeartRateController> _logger;

        public HeartRateController(ILogger<HeartRateController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "心率监测WebSocket服务正在运行。请使用WebSocket连接到'/ws'路径。" });
        }
    }
}
